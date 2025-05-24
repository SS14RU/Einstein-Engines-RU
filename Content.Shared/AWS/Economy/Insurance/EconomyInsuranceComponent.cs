using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.AWS.Economy.Insurance;

[RegisterComponent, NetworkedComponent]
public sealed partial class EconomyInsuranceComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextIconCheck { get; set; }

    [ViewVariables(VVAccess.ReadWrite)]
    public EconomyInsuranceIconPrototype Icon;
}
