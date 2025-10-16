using Content.Client.Cargo.BUI;
using Content.Client.Cargo.UI;
using Content.Shared.AWS.Economy.Cargo;
using Content.Shared.Cargo.BUI;

namespace Content.Client.Cargo.BUI;

public sealed partial class CargoOrderConsoleBoundUserInterface
{
    private string? _awsCurrencyId;

    partial void AwsOnMenuOpened(CargoConsoleMenu menu)
    {
        menu.AwsSetCurrency(_awsCurrencyId);
    }

    partial void AwsOnStateUpdated(CargoConsoleInterfaceState state)
    {
        if (state is CargoConsoleAwsInterfaceState awsState)
        {
            _awsCurrencyId = awsState.CurrencyPrototype;
            _menu?.AwsSetCurrency(_awsCurrencyId);
            return;
        }

        _awsCurrencyId = null;
        _menu?.AwsSetCurrency(_awsCurrencyId);
    }
}
