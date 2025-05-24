using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.AWS.Economy.SellableBatteries
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class SellableBatteryProxySwitcherComponent : Component
    {
        [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField] public bool Connected { get; set; }
        [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField] public EntityUid ConnectedBattery { get; set; }
    }
}
