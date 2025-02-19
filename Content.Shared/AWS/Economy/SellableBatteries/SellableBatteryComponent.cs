using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.AWS.Economy.SellableBatteries
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
    public sealed partial class SellableBatteryComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite), DataField]
        [AutoNetworkedField]
        public bool Charged { get; set; } = false;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        [AutoNetworkedField]
        public ulong Capacity { get; set; } = 10000;
    }
}
