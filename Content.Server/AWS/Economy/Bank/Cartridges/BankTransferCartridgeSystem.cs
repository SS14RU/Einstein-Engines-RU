using System;
using Content.Server.AWS.Economy.Bank;
using Content.Server.CartridgeLoader;
using Content.Shared.AWS.Economy.Bank;
using Content.Shared.AWS.Economy.Bank.Cartridges;
using Content.Shared.CartridgeLoader;
using Content.Shared.PDA;
using Robust.Shared.Containers;
using Robust.Shared.Localization;
using Robust.Shared.Network;
using Robust.Shared.GameObjects;

namespace Content.Server.AWS.Economy.Bank.Cartridges;

public sealed class BankTransferCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly EconomyBankAccountSystem _bankSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BankTransferCartridgeComponent, CartridgeAddedEvent>(OnCartridgeAdded);
        SubscribeLocalEvent<BankTransferCartridgeComponent, CartridgeActivatedEvent>(OnCartridgeActivated);
        SubscribeLocalEvent<BankTransferCartridgeComponent, CartridgeUiReadyEvent>(OnCartridgeUiReady);
        SubscribeLocalEvent<BankTransferCartridgeComponent, CartridgeMessageEvent>(OnCartridgeUiMessage);
        SubscribeLocalEvent<EconomyAccountHolderComponent, EntGotInsertedIntoContainerMessage>(OnAccountHolderInserted,
            after: new[] { typeof(SharedPdaSystem) });
        SubscribeLocalEvent<EconomyAccountHolderComponent, EntGotRemovedFromContainerMessage>(OnAccountHolderRemoved,
            after: new[] { typeof(SharedPdaSystem) });
    }

    private void OnCartridgeAdded(EntityUid uid, BankTransferCartridgeComponent component, ref CartridgeAddedEvent args)
    {
        component.Loader = args.Loader;
        EnsureReadonly(uid);
        UpdateUi(uid, component, args.Loader);
    }

    private void OnCartridgeActivated(EntityUid uid, BankTransferCartridgeComponent component, ref CartridgeActivatedEvent args)
    {
        component.Loader = args.Loader;
        EnsureReadonly(uid);
        UpdateUi(uid, component, args.Loader);
    }

    private void OnCartridgeUiReady(EntityUid uid, BankTransferCartridgeComponent component, ref CartridgeUiReadyEvent args)
    {
        component.Loader = args.Loader;
        EnsureReadonly(uid);
        UpdateUi(uid, component, args.Loader);
    }

    private void OnCartridgeUiMessage(EntityUid uid, BankTransferCartridgeComponent component, ref CartridgeMessageEvent args)
    {
        if (args is not BankTransferCartridgeUiMessageEvent transfer)
            return;

        var loader = ResolveLoader(ref component.Loader, transfer.LoaderUid);
        if (loader == EntityUid.Invalid)
            return;

        if (!TryGetAccountHolder(loader, out var holder))
        {
            component.PendingError = Loc.GetString("economybanksystem-transaction-error-notfoundaccout");
            UpdateUi(uid, component, loader);
            return;
        }

        if (transfer.Amount == 0)
        {
            component.PendingError = Loc.GetString("economybanksystem-transaction-error-notenoughmoney");
            UpdateUi(uid, component, loader);
            return;
        }

        var recipientId = CanonicalizeAccountId(transfer.RecipientAccountId);

        string? error;
        _bankSystem.TrySendMoney(holder.Comp, recipientId, transfer.Amount, null, out error);

        component.PendingError = error;
        UpdateUi(uid, component, loader);
    }

    private void OnAccountHolderInserted(EntityUid uid, EconomyAccountHolderComponent component, ref EntGotInsertedIntoContainerMessage args)
    {
        var owner = args.Container.Owner;
        if (owner == EntityUid.Invalid || !HasComp<PdaComponent>(owner))
            return;

        if (args.Container.ID != PdaComponent.PdaIdSlotId)
            return;

        RefreshCartridges(owner);
    }

    private void OnAccountHolderRemoved(EntityUid uid, EconomyAccountHolderComponent component, ref EntGotRemovedFromContainerMessage args)
    {
        var owner = args.Container.Owner;
        if (owner == EntityUid.Invalid || !HasComp<PdaComponent>(owner))
            return;

        if (args.Container.ID != PdaComponent.PdaIdSlotId)
            return;

        RefreshCartridges(owner);
    }

    private void RefreshCartridges(EntityUid loaderUid)
    {
        if (!TryComp(loaderUid, out CartridgeLoaderComponent? loader))
            return;

        if (loader.CartridgeSlot.Item is { } inserted && TryComp(inserted, out BankTransferCartridgeComponent? insertedComp))
            UpdateUi(inserted, insertedComp, loaderUid);

        foreach (var program in _cartridgeLoader.GetInstalled(loaderUid))
        {
            if (!TryComp(program, out BankTransferCartridgeComponent? installedComp))
                continue;

            UpdateUi(program, installedComp, loaderUid);
        }
    }

    private void EnsureReadonly(EntityUid cartridgeUid)
    {
        if (!TryComp<CartridgeComponent>(cartridgeUid, out var cartridge))
            return;

        if (cartridge.InstallationStatus == InstallationStatus.Readonly)
            return;

        cartridge.InstallationStatus = InstallationStatus.Readonly;
        Dirty(cartridgeUid, cartridge);
    }

    private void UpdateUi(EntityUid cartridgeUid, BankTransferCartridgeComponent component, EntityUid loaderUid)
    {
        component.Loader = loaderUid;

        EconomyBankATMAccountInfo? accountInfo = null;
        if (TryBuildAccountInfo(loaderUid, out var info))
            accountInfo = info;

        var error = component.PendingError;
        if (accountInfo is null)
            error = null;

        var state = new EconomyBankATMUserInterfaceState
        {
            BankAccount = accountInfo,
            Error = error
        };

        _cartridgeLoader.UpdateCartridgeUiState(loaderUid, state);
        component.PendingError = null;
    }

    private bool TryGetAccountHolder(EntityUid loaderUid, out Entity<EconomyAccountHolderComponent> holder)
    {
        holder = default;
        if (!TryComp(loaderUid, out PdaComponent? pda) || pda.ContainedId is not { } id)
            return false;

        if (!TryComp(id, out EconomyAccountHolderComponent? holderComp) || holderComp is null)
            return false;

        holder = (id, holderComp);
        return true;
    }

    private EntityUid ResolveLoader(ref EntityUid? storedLoader, NetEntity loaderNet)
    {
        if (storedLoader is { } existing && existing != EntityUid.Invalid)
            return existing;

        if (loaderNet == default)
            return EntityUid.Invalid;

        var resolved = GetEntity(loaderNet);
        storedLoader = resolved;
        return resolved;
    }

    private bool TryBuildAccountInfo(EntityUid loaderUid, out EconomyBankATMAccountInfo info)
    {
        info = default!;

        if (!TryGetAccountHolder(loaderUid, out var holder))
            return false;

        return _bankSystem.TryBuildAccountInfo(holder.Comp, out info);
    }

    private string CanonicalizeAccountId(string rawId)
    {
        var trimmed = rawId.Trim();

        if (_bankSystem.TryGetAccount(trimmed, out _))
            return trimmed;

        var accounts = _bankSystem.GetAccounts(EconomyBankAccountMask.All);

        foreach (var accountId in accounts.Keys)
        {
            if (string.Equals(accountId, trimmed, StringComparison.OrdinalIgnoreCase))
                return accountId;
        }

        static string StripPrefix(string id)
        {
            var index = id.IndexOf('-', StringComparison.Ordinal);
            return index >= 0 ? id[(index + 1)..] : id;
        }

        var strippedInput = StripPrefix(trimmed);

        foreach (var accountId in accounts.Keys)
        {
            if (string.Equals(StripPrefix(accountId), strippedInput, StringComparison.OrdinalIgnoreCase))
                return accountId;
        }

        return trimmed;
    }
}
