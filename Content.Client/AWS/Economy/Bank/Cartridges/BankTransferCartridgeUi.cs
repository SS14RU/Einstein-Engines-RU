using Content.Client.UserInterface.Fragments;
using Content.Shared.AWS.Economy.Bank;
using Content.Shared.AWS.Economy.Bank.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Client.UserInterface;

namespace Content.Client.AWS.Economy.Bank.Cartridges;

public sealed partial class BankTransferCartridgeUi : UIFragment
{
    private BankTransferUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new BankTransferUiFragment();
        _fragment.TransferRequested += (amount, recipient) =>
        {
            var message = new BankTransferCartridgeUiMessageEvent(amount, recipient);
            var envelope = new CartridgeUiMessage(message);
            userInterface.SendMessage(envelope);
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not EconomyBankATMUserInterfaceState atmState)
            return;

        _fragment?.UpdateState(atmState);
    }
}
