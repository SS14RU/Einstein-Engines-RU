using Content.Client.Cargo.UI;
using Content.Shared.AWS.Economy.Cargo;
using Content.Shared.Cargo.BUI;

namespace Content.Client.Cargo.BUI;

public sealed partial class CargoOrderConsoleBoundUserInterface
{
    private string? _awsCurrencyId;

    partial void OnMenuOpenedExtended(CargoConsoleMenu menu)
    {
        menu.SetCurrency(_awsCurrencyId);
    }

    partial void OnStateUpdatedExtended(CargoConsoleInterfaceState state)
    {
        if (state is CargoConsoleAwsInterfaceState awsState)
        {
            _awsCurrencyId = awsState.CurrencyPrototype;
            _menu?.SetCurrency(_awsCurrencyId);
            return;
        }

        _awsCurrencyId = null;
        _menu?.SetCurrency(_awsCurrencyId);
    }
}
