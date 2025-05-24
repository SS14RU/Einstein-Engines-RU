using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.AWS.Economy.SellableBatteries
{
    [RegisterComponent]
    public sealed partial class SellableBatteryComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite), DataField]
        public ulong PricePerChargedPrecent { get; set; } = 0;
    }
}
