using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.AWS.Economy.SellableBatteries
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
    public sealed partial class SellableBatteryChargerComponent : Component
    {
    }
}
