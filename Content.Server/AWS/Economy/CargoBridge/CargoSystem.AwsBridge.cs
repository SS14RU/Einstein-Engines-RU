using Content.Server.AWS.Economy.Bank;
using Content.Server.AWS.Economy.CargoBridge;
using Content.Server.Cargo.Components;
using Content.Shared.AWS.Economy.Cargo;
using Content.Shared.Cargo.BUI;
using Robust.Shared.Log;

namespace Content.Server.Cargo.Systems;

public sealed partial class CargoSystem
{
    [Dependency] private readonly EconomyBankAccountSystem _economyBankAccount = default!;

    private EntityQuery<StationCargoAwsAccountComponent> _cargoAccountQuery = default!;
    private ISawmill _awsBridgeSawmill = default!;

    partial void InitializeAwsBridge()
    {
        _awsBridgeSawmill = Logger.GetSawmill("cargo.aws_bridge");
        _cargoAccountQuery = GetEntityQuery<StationCargoAwsAccountComponent>();

        SubscribeLocalEvent<StationBankAccountComponent, ComponentStartup>(OnStationBankStartup);
        SubscribeLocalEvent<StationCargoAwsAccountComponent, ComponentStartup>(OnCargoAccountStartup);
    }

    private void OnStationBankStartup(EntityUid uid, StationBankAccountComponent component, ref ComponentStartup args)
    {
        TrySyncStationBalance(uid, component);
    }

    private void OnCargoAccountStartup(EntityUid uid, StationCargoAwsAccountComponent component, ref ComponentStartup args)
    {
        if (!TryComp(uid, out StationBankAccountComponent? bank))
            return;

        TrySyncStationBalance(uid, bank, component);
    }

    partial void BeforeCargoBankUpdate(EntityUid station, StationBankAccountComponent component, ref int amount, ref bool handled)
    {
        if (!_cargoAccountQuery.TryGetComponent(station, out var cargoAccount))
            return;

        if (!TryApplyAccountDelta(station, component, cargoAccount, amount))
            return;

        handled = true;
    }

    partial void AfterCargoBankUpdate(EntityUid station, StationBankAccountComponent component, int amount, bool handled)
    {
        TrySyncStationBalance(station, component);
    }

    private bool TryApplyAccountDelta(EntityUid station, StationBankAccountComponent bank, StationCargoAwsAccountComponent cargoAccount, int delta)
    {
        if (delta == 0)
        {
            TrySyncStationBalance(station, bank, cargoAccount);
            return true;
        }

        if (string.IsNullOrWhiteSpace(cargoAccount.AccountId))
        {
            _awsBridgeSawmill.Warning($"Station {station} has StationCargoAwsAccountComponent without an AccountId.");
            return false;
        }

        if (!_economyBankAccount.TryChangeAccountBalance(cargoAccount.AccountId, delta))
        {
            _awsBridgeSawmill.Warning($"Failed to adjust AWS account {cargoAccount.AccountId} by {delta} for station {station}.");
            return false;
        }

        TrySyncStationBalance(station, bank, cargoAccount);
        return true;
    }

    private void TrySyncStationBalance(EntityUid station, StationBankAccountComponent bank)
    {
        if (!_cargoAccountQuery.TryGetComponent(station, out var cargoAccount))
            return;

        TrySyncStationBalance(station, bank, cargoAccount);
    }

    private void TrySyncStationBalance(EntityUid station, StationBankAccountComponent bank, StationCargoAwsAccountComponent cargoAccount)
    {
        if (string.IsNullOrWhiteSpace(cargoAccount.AccountId))
            return;

        if (!_economyBankAccount.TryGetAccount(cargoAccount.AccountId, out var account))
        {
            _awsBridgeSawmill.Warning($"Unable to locate AWS account {cargoAccount.AccountId} for station {station}.");
            return;
        }

        var newBalance = account.Value.Comp.Balance > int.MaxValue
            ? int.MaxValue
            : (int) account.Value.Comp.Balance;

        if (bank.Balance == newBalance)
            return;

        bank.Balance = newBalance;
    }

    partial void ShouldSkipCargoPassiveIncome(EntityUid station, StationBankAccountComponent bank, ref bool skip)
    {
        if (_cargoAccountQuery.HasComponent(station))
            skip = true;
    }

    partial void AdjustCargoInterfaceState(EntityUid station, StationCargoOrderDatabaseComponent orderDatabase, StationBankAccountComponent bankAccount, ref CargoConsoleInterfaceState state)
    {
        if (!_cargoAccountQuery.TryGetComponent(station, out var account))
            return;

        state = new CargoConsoleAwsInterfaceState(
            state.Name,
            state.Count,
            state.Capacity,
            state.Balance,
            orderDatabase.Orders,
            account.Currency.Id);
    }
}
