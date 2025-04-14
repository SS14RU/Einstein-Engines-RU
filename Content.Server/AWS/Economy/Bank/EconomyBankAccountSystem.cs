using Content.Shared.Interaction;
using Content.Shared.VendingMachines;
using Content.Server.VendingMachines;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Content.Shared.AWS.Economy;
using Content.Server.Popups;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Content.Shared.Access.Components;
using Content.Server.Access.Components;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using System.Diagnostics.CodeAnalysis;
using Content.Server.Station.Systems;
using Robust.Server.GameStates;
using Content.Shared.Store;
using Content.Shared.Access.Systems;
using System.Linq;
using Content.Shared.Roles;
using Content.Shared.AWS.Economy.Bank;

namespace Content.Server.AWS.Economy.Bank
{
    public sealed class EconomyBankAccountSystem : EconomyBankAccountSystemShared
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly VendingMachineSystem _vendingMachine = default!;
        [Dependency] private readonly INetManager _netManager = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly StationSystem _stationSystem = default!;
        [Dependency] private readonly PvsOverrideSystem _pvsOverrideSystem = default!;
        [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
        [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<EconomyAccountHolderComponent, ComponentInit>(OnAccountComponentInit);
            SubscribeLocalEvent<EconomyBankAccountComponent, ComponentRemove>(OnBankComponentRemove);

            SubscribeLocalEvent<EconomyBankTerminalComponent, InteractUsingEvent>(OnTerminalInteracted);

            SubscribeLocalEvent<EconomyBankATMComponent, GotEmaggedEvent>(OnATMEmagged);
            SubscribeLocalEvent<EconomyBankATMComponent, InteractUsingEvent>(OnATMInteracted);
            SubscribeLocalEvent<EconomyBankATMComponent, EconomyBankATMWithdrawMessage>(OnATMWithdrawMessage);
            SubscribeLocalEvent<EconomyBankATMComponent, EconomyBankATMTransferMessage>(OnATMTransferMessage);

            SubscribeLocalEvent<EconomyManagementConsoleComponent, EconomyManagementConsoleChangeParameterMessage>(OnManagementConsoleParameterMessage);
            SubscribeLocalEvent<EconomyManagementConsoleComponent, EconomyManagementConsoleChangeHolderIDMessage>(OnManagementConsoleChangeHolderIDMessage);
            SubscribeLocalEvent<EconomyManagementConsoleComponent, EconomyManagementConsoleInitAccountOnHolderMessage>(OnManagementConsoleInitAccountOnHolderMessage);
            SubscribeLocalEvent<EconomyManagementConsoleComponent, EconomyManagementConsolePayBonusMessage>(OnManagementConsolePayBonusMessage);
        }

        #region Account management
        /// <summary>
        /// Creates a new bank account entity. If account already exists - fetch it instead, but still return false.
        /// </summary>
        /// <returns>Whether the account was successfully created.</returns>
        [PublicAPI]
        public bool TryCreateAccount(string accountID,
                                     string accountName,
                                     ProtoId<CurrencyPrototype> allowedCurrency,
                                     ulong balance,
                                     ulong penalty,
                                     bool blocked,
                                     bool canReachPayDay,
                                     List<BankAccountTag>? accountTags,
                                     ProtoId<JobPrototype>? jobName,
                                     ulong? salary,
                                     MapCoordinates? cords,
                                     out Entity<EconomyBankAccountComponent> account)
        {
            // Return if account with this id already exists.
            if (TryGetAccount(accountID, out var foundAccount))
            {
                account = foundAccount;
                return false;
            }

            var spawnCords = cords ?? MapCoordinates.Nullspace;
            var accountEntity = Spawn(null, spawnCords);
            _metaDataSystem.SetEntityName(accountEntity, accountID);
            var accountComp = EnsureComp<EconomyBankAccountComponent>(accountEntity);

            accountComp.AccountID = accountID;
            accountComp.AccountName = accountName;
            accountComp.AllowedCurrency = allowedCurrency;
            accountComp.Balance = balance;
            accountComp.Penalty = penalty;
            accountComp.Blocked = blocked;
            accountComp.CanReachPayDay = canReachPayDay;
            accountComp.AccountTags = accountTags ?? [];
            accountComp.JobName = jobName;
            accountComp.Salary = salary;

            account = (accountEntity, accountComp);
            _pvsOverrideSystem.AddGlobalOverride(accountEntity);
            Dirty(account);
            return true;
        }

        /// <summary>
        /// Enables a card or a bank account (described in setup) for usage.
        /// </summary>
        [PublicAPI]
        public bool TryActivate(Entity<EconomyAccountHolderComponent> entity, [NotNullWhen(true)] out Entity<EconomyBankAccountComponent>? activatedAccount)
        {
            activatedAccount = null;
            if (!_prototypeManager.TryIndex(entity.Comp.AccountIdByProto, out EconomyAccountIdPrototype? proto))
                return false;

            // Setup standard starting values for account details
            var accountID = GenerateAccountId(proto.Prefix, proto.Streak, proto.NumbersPerStreak, proto.Descriptior);
            var accountName = entity.Comp.AccountName;
            var balance = (ulong)0;
            ProtoId<JobPrototype>? jobName = null;
            ulong? salary = null;

            if (TryComp<IdCardComponent>(entity, out var idCardComponent))
                accountName = idCardComponent.FullName ?? entity.Comp.AccountName;

            if (TryComp<PresetIdCardComponent>(entity, out var presetIdCardComponent) &&
                presetIdCardComponent.JobName is { } job &&
                TryGetSalaryJobEntry(job, "NanotrasenDefaultSallaries", out var jobEntry))
            {
                jobName = job;
                salary = jobEntry.Value.Sallary;
                balance = (ulong)(jobEntry.Value.StartMoney * _robustRandom.NextDouble(0.5, 1.5));
            }

            var station = _stationSystem.GetOwningStation(entity);
            var cords = station != null ? _transformSystem.GetMapCoordinates(station.Value) : MapCoordinates.Nullspace;

            // Setup values are always coming first if they can
            var accountSetup = entity.Comp.AccountSetup;
            accountID = accountSetup.GenerateAccountID || accountSetup.AccountID is null ? accountID :
                                        accountSetup.AccountID;
            accountName = accountSetup.AccountName ?? accountName;
            balance = accountSetup.Balance ?? balance;

            TryCreateAccount(accountID,
                            accountName,
                            accountSetup.AllowedCurrency ?? "Thaler",
                            balance,
                            accountSetup.Penalty ?? 0,
                            accountSetup.Blocked ?? false,
                            accountSetup.CanReachPayDay ?? true,
                            accountSetup.AccountTags ?? [],
                            jobName,
                            salary,
                            cords,
                            out var account);

            activatedAccount = account;
            entity.Comp.AccountID = accountID;
            entity.Comp.AccountName = accountName;
            Dirty(entity);
            return true;
        }

        /// <summary>
        /// Tries to set the parameter chosen in arguments to a given value.
        /// </summary>
        /// <param name="param">Parameter to be changed.</param>
        /// <param name="value">New value of the parameter, must be of the same type as a changed value.</param>
        [PublicAPI]
        public bool TrySetAccountParameter(string accountID, EconomyBankAccountParam param, object value)
        {
            if (!TryGetAccount(accountID, out var entity))
                return false;

            var account = entity.Comp;
            switch (param)
            {
                case EconomyBankAccountParam.AccountName:
                    if (value is not string name)
                        return false;
                    account.AccountName = name;
                    break;
                case EconomyBankAccountParam.Blocked:
                    if (value is not bool blocked)
                        return false;
                    account.Blocked = blocked;
                    break;
                case EconomyBankAccountParam.CanReachPayDay:
                    if (value is not bool canReachPayDay)
                        return false;
                    account.CanReachPayDay = canReachPayDay;
                    break;
                case EconomyBankAccountParam.JobName:
                    if (value is not string jobName)
                        return false;
                    account.JobName = jobName;
                    break;
                case EconomyBankAccountParam.Salary:
                    if (value is not ulong salary)
                        return false;
                    account.Salary = salary;
                    break;
                default:
                    return false;
            }

            Dirty(entity);
            return true;
        }

        [PublicAPI]
        public bool TryWithdraw(EconomyAccountHolderComponent component, EconomyBankATMComponent atm, ulong sum, [NotNullWhen(false)] out string? errorMessage)
        {
            errorMessage = null;
            if (!TryGetAccount(component.AccountID, out var account))
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notfoundaccout");
                return false;
            }

            if (account.Comp.Blocked)
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-account-blocked", ("accountId", account.Comp.AccountID));
                return false;
            }

            if (sum > 0 && account.Comp.Balance >= sum)
            {
                Withdraw(component, atm, sum);
                return true;
            }
            errorMessage = Loc.GetString("economybanksystem-transaction-error-notenoughmoney");
            return false;
        }

        [PublicAPI]
        public Entity<EconomyMoneyHolderComponent> DropMoneyHandler(EntProtoId<EconomyMoneyHolderComponent> entId, ulong amount, MapCoordinates pos)
        {
            var ent = Spawn(entId, pos);

            var moneyHolderComp = Comp<EconomyMoneyHolderComponent>(ent);
            moneyHolderComp.Balance = amount;

            _entManager.Dirty(moneyHolderComp);

            return (ent, moneyHolderComp);
        }

        [PublicAPI]
        public bool TrySendMoney(IEconomyMoneyHolder fromHolder, EconomyAccountHolderComponent? recipientAccount, ulong amount, [NotNullWhen(false)] out string? errorMessage)
        {
            errorMessage = null;

            if (fromHolder.Balance < amount)
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notenoughmoney");
                return false;
            }

            if (recipientAccount is null || !TryGetAccount(recipientAccount.AccountID, out var account))
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notfoundaccout");
                return false;
            }

            if (account.Comp.Blocked)
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-account-blocked", ("accountId", account.Comp.AccountID));
                return false;
            }

            return TryChangeAccountBalance(recipientAccount.AccountID, amount);
        }

        [PublicAPI]
        public bool TrySendMoney(IEconomyMoneyHolder fromHolder, string recipientAccountId, ulong amount, [NotNullWhen(false)] out string? errorMessage)
        {
            errorMessage = null;

            if (!TryGetAccount(recipientAccountId, out var account))
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notfoundaccout", ("accountId", recipientAccountId));
                return false;
            }

            if (account.Comp.Blocked)
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-account-blocked", ("accountId", account.Comp.AccountID));
                return false;
            }

            if (fromHolder.Balance < amount)
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notenoughmoney");
                return false;
            }

            return TryChangeAccountBalance(recipientAccountId, amount);
        }

        [PublicAPI]
        public bool TrySendMoney(EconomyAccountHolderComponent fromAccountHolder, string recipientAccountId, ulong amount, string? reason, [NotNullWhen(false)] out string? errorMessage)
        {
            errorMessage = null;

            if (!TryGetAccount(fromAccountHolder.AccountID, out var fromBankAccount))
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notfoundaccout", ("accountId", fromAccountHolder.AccountID));
                return false;
            }

            if (!TryGetAccount(recipientAccountId, out var recipientAccount))
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notfoundaccout", ("accountId", recipientAccountId));
                return false;
            }

            if (fromBankAccount.Comp.Blocked || recipientAccount.Comp.Blocked)
            {
                var blockedAccountID = fromBankAccount.Comp.Blocked ? fromBankAccount.Comp.AccountID : recipientAccount.Comp.AccountID;
                errorMessage = Loc.GetString("economybanksystem-transaction-error-account-blocked", ("accountId", blockedAccountID));
                return false;
            }

            if (fromBankAccount.Comp.Balance < amount)
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notenoughmoney");
                return false;
            }

            return TryTransferMoney(fromBankAccount.Comp.AccountID, recipientAccountId, amount, reason);
        }

        [PublicAPI]
        public bool TrySendMoney(string fromAccountId, string recipientAccountId, ulong amount, string? reason, [NotNullWhen(false)] out string? errorMessage)
        {
            errorMessage = null;

            if (!TryGetAccount(fromAccountId, out var fromBankAccount))
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notfoundaccout", ("accountId", fromAccountId));
                return false;
            }

            if (!TryGetAccount(recipientAccountId, out var recipientAccount))
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notfoundaccout", ("accountId", recipientAccountId));
                return false;
            }

            if (fromBankAccount.Comp.Blocked || recipientAccount.Comp.Blocked)
            {
                var blockedAccountID = fromBankAccount.Comp.Blocked ? fromAccountId : recipientAccountId;
                errorMessage = Loc.GetString("economybanksystem-transaction-error-account-blocked", ("accountId", blockedAccountID));
                return false;
            }

            if (fromBankAccount.Comp.Balance < amount)
            {
                errorMessage = Loc.GetString("economybanksystem-transaction-error-notenoughmoney");
                return false;
            }

            return TryTransferMoney(fromAccountId, recipientAccountId, amount, reason);
        }

        /// <summary>
        /// Adds a new log to the account.
        /// </summary>
        [PublicAPI]
        public void AddLog(string accountID, EconomyBankAccountLogField log)
        {
            if (!TryGetAccount(accountID, out var account))
                return;

            account.Comp.Logs.Add(log);
            Dirty(account);
        }

        /// <summary>
        /// Changes the balance of the account.
        /// </summary>
        /// <param name="addition">Whether to add or substract the given amount.</param>
        private bool TryChangeAccountBalance(string accountID, ulong amount, bool addition = true)
        {
            if (!TryGetAccount(accountID, out var entity))
                return false;

            var account = entity.Comp;
            if (!addition)
            {
                if (account.Balance - amount < 0)
                    return false;

                account.Balance -= amount;
                return true;
            }

            account.Balance += amount;

            Dirty(entity);
            return true;
        }

        /// <summary>
        /// Transfer money from one account to another (with logs).
        /// </summary>
        /// <returns>True if the transfer was successful, false otherwise.</returns>
        private bool TryTransferMoney(string senderID, string receiverID, ulong amount, string? reason = null)
        {
            if (amount <= 0 ||
                !TryGetAccount(senderID, out var senderEntity) ||
                !TryGetAccount(receiverID, out var receiverEntity))
                return false;

            var sender = senderEntity.Comp;
            var receiver = receiverEntity.Comp;
            if (sender.Balance < amount)
                return false;

            sender.Balance -= amount;
            receiver.Balance += amount;

            var senderLog = Loc.GetString("economybanksystem-log-send-to",
                        ("amount", amount), ("currencyName", receiver.AllowedCurrency), ("accountId", receiver.AccountID));
            var receiverLog = Loc.GetString("economybanksystem-log-send-from",
                        ("amount", amount), ("currencyName", receiver.AllowedCurrency), ("accountId", sender.AccountID));
            if (reason != null)
            {
                senderLog += $" {reason}";
                receiverLog += $" {reason}";
            }
            sender.Logs.Add(new(_gameTiming.CurTime, senderLog));
            receiver.Logs.Add(new(_gameTiming.CurTime, receiverLog));

            Dirty(senderEntity);
            Dirty(receiverEntity);
            return true;
        }

        private void Withdraw(EconomyAccountHolderComponent component, EconomyBankATMComponent atm, ulong sum)
        {
            if (!TryChangeAccountBalance(component.AccountID, sum, false))
                return;

            var pos = _transformSystem.GetMapCoordinates(atm.Owner);
            DropMoneyHandler(component.MoneyHolderEntId, sum, pos);

            if (TryGetAccount(component.AccountID, out var account))
            {
                var log = new EconomyBankAccountLogField(_gameTiming.CurTime, Loc.GetString("economybanksystem-log-withdraw",
                ("amount", sum), ("currencyName", account.Comp.AllowedCurrency)));
                account.Comp.Logs.Add(log);
                Dirty(account);
            }

            _entManager.Dirty(component);
        }

        private void Withdraw(string accountID, EntityUid ent, ulong sum)
        {
            if (!TryChangeAccountBalance(accountID, sum, false))
                return;

            var pos = _transformSystem.GetMapCoordinates(ent);
            DropMoneyHandler("ThalerHolder", sum, pos); // hardcoded for now

            if (TryGetAccount(accountID, out var account))
            {
                var log = new EconomyBankAccountLogField(_gameTiming.CurTime, Loc.GetString("economybanksystem-log-withdraw",
                ("amount", sum), ("currencyName", account.Comp.AllowedCurrency)));
                account.Comp.Logs.Add(log);
                Dirty(account);
            }
        }
        #endregion

        private string GenerateAccountId(string prefix, uint strik, uint numbersPerStrik, string? descriptor)
        {
            var res = prefix;

            for (int i = 0; i < strik; i++)
            {
                string formedStrik = "";

                for (int num = 0; num < numbersPerStrik; num++)
                {
                    formedStrik += _robustRandom.Next(0, 10);
                }

                res = res.Length == 0 ? formedStrik : res + descriptor + formedStrik;
            }

            return res;
        }

        private void OnAccountComponentInit(Entity<EconomyAccountHolderComponent> entity, ref ComponentInit args)
        {
            // if has id card comp, then it will be initialized in the other place.
            if (entity.Comp.AccountSetup is null || HasComp<IdCardComponent>(entity))
                return;

            TryActivate(entity, out _);
        }

        private void OnBankComponentRemove(Entity<EconomyBankAccountComponent> entity, ref ComponentRemove args)
        {
            _pvsOverrideSystem.RemoveGlobalOverride(entity);
        }

        private void OnATMWithdrawMessage(EntityUid uid, EconomyBankATMComponent atm, EconomyBankATMWithdrawMessage args)
        {
            var bankAccount = GetATMInsertedAccount(atm);
            if (bankAccount is null)
                return;

            string? error;

            TryWithdraw(bankAccount, atm, args.Amount, out error);
            UpdateATMUserInterface((uid, atm), error);
        }

        private void OnATMTransferMessage(EntityUid uid, EconomyBankATMComponent atm, EconomyBankATMTransferMessage args)
        {
            var bankAccount = GetATMInsertedAccount(atm);
            if (bankAccount is null)
                return;

            string? error;

            TrySendMoney(bankAccount, args.RecipientAccountId, args.Amount, null, out error);
            UpdateATMUserInterface((uid, atm), error);
        }

        private void OnATMEmagged(EntityUid uid, EconomyBankATMComponent component, ref GotEmaggedEvent args)
        {
            if (HasComp<EmaggedComponent>(uid) || args.Handled)
                return;

            var listMoney = component.EmagDropMoneyValues;
            var listMoneyCount = listMoney.Count;

            if (listMoneyCount == 0)
                return;

            if (component.EmagDropMoneyHolderRandomCount == 0)
                return;

            var moneyHolderCount = _robustRandom.Next(1, component.EmagDropMoneyHolderRandomCount + 1);
            var mapPos = _transformSystem.GetMapCoordinates(uid);

            for (int i = 0; i < moneyHolderCount; i++)
            {
                var droppedEnt = DropMoneyHandler(component.MoneyHolderEntId,
                    listMoney[_robustRandom.Next(0, listMoneyCount)], mapPos);
                droppedEnt.Comp.Emagged = true;
            }

            _audioSystem.PlayPvs(component.EmagSound, uid);
            args.Handled = true;
        }

        private void OnATMInteracted(EntityUid uid, EconomyBankATMComponent component, InteractUsingEvent args)
        {
            var usedEnt = args.Used;

            if (!TryComp<EconomyMoneyHolderComponent>(usedEnt, out var economyMoneyHolderComponent))
                return;

            var amount = economyMoneyHolderComponent.Balance;
            var insertedAccountComponent = GetATMInsertedAccount(component);

            if (TrySendMoney(economyMoneyHolderComponent, insertedAccountComponent, amount, out var error))
            {
                if (insertedAccountComponent is not null && TryGetAccount(insertedAccountComponent.AccountID, out var account))
                    AddLog(account.Comp.AccountID,
                           new EconomyBankAccountLogField(_gameTiming.CurTime,
                           Loc.GetString("economybanksystem-log-insert",
                           ("amount", amount), ("currencyName", account.Comp.AllowedCurrency))));

                if (_netManager.IsServer)
                    _popupSystem.PopupEntity(Loc.GetString("economybanksystem-atm-moneyentering"), uid, type: PopupType.Medium);

                QueueDel(usedEnt);
            }
            if (_netManager.IsServer)
                _popupSystem.PopupEntity(error, uid, type: PopupType.Medium);

            UpdateATMUserInterface((uid, component), error);
        }

        private void OnTerminalInteracted(EntityUid uid, EconomyBankTerminalComponent component, InteractUsingEvent args)
        {
            var amount = component.Amount;
            var usedEnt = args.Used;

            if (amount <= 0)
                return;

            if (!TryComp<EconomyMoneyHolderComponent>(usedEnt, out var economyMoneyHolderComponent) &
                !TryComp<EconomyAccountHolderComponent>(usedEnt, out var economyBankAccountHolderComponent))
                return;

            if (!TryGetAccount(component.LinkedAccount, out var receiverAccount))
            {
                var error = Loc.GetString("economyBankTerminal-component-vending-error-no-account");
                _popupSystem.PopupEntity(error, uid, type: PopupType.MediumCaution);
                return;
            }

            if (economyMoneyHolderComponent is not null)
            {
                if (!TrySendMoney(economyMoneyHolderComponent, component.LinkedAccount, amount, out var err))
                {
                    _popupSystem.PopupEntity(err, uid, type: PopupType.MediumCaution);
                    return;
                }
            }
            else if (economyBankAccountHolderComponent is not null)
            {
                if (!TrySendMoney(economyBankAccountHolderComponent, component.LinkedAccount, amount, null, out var err))
                {
                    _popupSystem.PopupEntity(err, uid, type: PopupType.MediumCaution);
                    return;
                }
            }

            UpdateTerminal((uid, component), 0, string.Empty);

            // Cancel the payment if the terminal is vending machine and the further operations were not successful.
            if (TryComp<VendingMachineComponent>(uid, out var vendingMachineComponent) &&
                !TryTransactionFromVendingMachine(uid, args.User, vendingMachineComponent))
            {
                Withdraw(receiverAccount.Comp.AccountID, uid, amount);
                var error = Loc.GetString("economyBankTerminal-component-vending-error");
                _popupSystem.PopupEntity(error, uid, type: PopupType.MediumCaution);
                return;
            }

            _popupSystem.PopupEntity(Loc.GetString("economybanksystem-transaction-success", ("amount", amount), ("currencyName", receiverAccount.Comp.AllowedCurrency)), uid, type: PopupType.Medium);
        }

        private bool TryTransactionFromVendingMachine(EntityUid uid, EntityUid user, VendingMachineComponent vendingMachine)
        {
            if (vendingMachine.SelectedItemId is not { } selectedItemID)
                return false;

            if (!vendingMachine.Inventory.TryGetValue(selectedItemID, out var entry) || entry.Price <= 0)
                return false;

            vendingMachine.SelectedItemId = null;
            _vendingMachine.AuthorizedVend(uid, user, vendingMachine.SelectedItemInventoryType, selectedItemID, vendingMachine);
            return !vendingMachine.Denying;
        }

        private void OnManagementConsoleChangeHolderIDMessage(Entity<EconomyManagementConsoleComponent> ent, ref EconomyManagementConsoleChangeHolderIDMessage args)
        {
            if (!TryComp<AccessReaderComponent>(ent, out var accessReader) || ent.Comp.CardSlot.Item is not { } idCard)
                return;

            // Check for privileges
            if (!_accessReaderSystem.IsAllowed(idCard, ent.Owner, accessReader))
                return;

            var holder = GetEntity(args.AccountHolder);
            if (!TryComp<EconomyAccountHolderComponent>(holder, out var holderComp))
                return;

            // Change the holder ID
            if (!TryGetAccount(args.NewID, out var account))
                return;

            holderComp.AccountID = account.Comp.AccountID;
            holderComp.AccountName = account.Comp.AccountName;
            Dirty(holder, holderComp);
            UpdateManagementConsoleUserInterface(ent, account.Comp);
        }

        private void OnManagementConsoleInitAccountOnHolderMessage(Entity<EconomyManagementConsoleComponent> ent, ref EconomyManagementConsoleInitAccountOnHolderMessage args)
        {
            if (!TryComp<AccessReaderComponent>(ent, out var accessReader) || ent.Comp.CardSlot.Item is not { } idCard)
                return;

            // Check for privileges
            if (!_accessReaderSystem.IsAllowed(idCard, ent.Owner, accessReader))
                return;

            // Initialize account on holder
            var holder = GetEntity(args.AccountHolder);

            if (!TryComp<EconomyAccountHolderComponent>(holder, out var holderComp) || !TryActivate((holder, holderComp), out var account))
                return;

            holderComp.AccountID = account.Value.Comp.AccountID;
            holderComp.AccountName = account.Value.Comp.AccountName;
            Dirty(holder, holderComp);
            UpdateManagementConsoleUserInterface(ent, account.Value.Comp);
        }

        private void OnManagementConsoleParameterMessage(Entity<EconomyManagementConsoleComponent> ent, ref EconomyManagementConsoleChangeParameterMessage args)
        {
            if (!TryComp<AccessReaderComponent>(ent, out var accessReader) || ent.Comp.CardSlot.Item is not { } idCard)
                return;

            // Check for priveleges
            if (!_accessReaderSystem.IsAllowed(idCard, ent.Owner, accessReader))
                return;

            if (!TryGetAccount(args.AccountID, out var account))
                return;

            TrySetAccountParameter(args.AccountID, args.Param, args.Value);
            UpdateManagementConsoleUserInterface(ent, account.Comp);
        }

        private void OnManagementConsolePayBonusMessage(Entity<EconomyManagementConsoleComponent> ent, ref EconomyManagementConsolePayBonusMessage args)
        {
            // Check for priveleges
            if (!TryComp<AccessReaderComponent>(ent, out var accessReader) || ent.Comp.CardSlot.Item is not { } idCard)
                return;

            if (!_accessReaderSystem.IsAllowed(idCard, ent.Owner, accessReader))
                return;

            // Validate accounts and operation itself
            if (!TryGetAccount(args.Payer, out var payerAccount))
                return;

            var accounts = GetAccounts(EconomyBankAccountMask.ByTags, new List<BankAccountTag> { BankAccountTag.Personal });
            var accountList = args.Accounts;
            var intersectedAccounts = accounts.Where(account => accountList.Contains(account.Value.Comp.AccountID)).GetEnumerator();

            Dictionary<Entity<EconomyBankAccountComponent>, ulong> accountsToPay = new();
            ulong total = 0;
            while (intersectedAccounts.MoveNext())
            {
                var account = intersectedAccounts.Current.Value.Comp;
                if (account.Salary is null)
                    continue;

                var bonus = (ulong)(account.Salary * args.BonusPercent);
                total += bonus;
                accountsToPay.Add(intersectedAccounts.Current.Value, bonus);
            }

            if (total > payerAccount.Comp.Balance)
                return;

            // Proceed to payment
            var reason = Loc.GetString("economybanksystem-log-reason-bonus");
            foreach (var kvp in accountsToPay)
            {
                var account = kvp.Key.Comp;
                var bonus = kvp.Value;

                TrySendMoney(payerAccount.Comp.AccountID, account.AccountID, bonus, reason, out _);
            }

            UpdateManagementConsoleUserInterface(ent, null);
        }
    }
}
