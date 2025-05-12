using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.AWS.Economy.SellableBatteries
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class SellableBatteryProxySwitcherComponent : Component
    {
        [AutoNetworkedField] public bool Connected { get; set; }
        [AutoNetworkedField] public EntityUid ConnectedBattery { get; set; }
    }
}
