using Content.Shared.Store;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Server.AWS.CriminalAntag;

[RegisterComponent]
public sealed partial class StealMoneyConditionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public StealMoneyReachType ReachType { get; set; } = StealMoneyReachType.AsPossible;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<CurrencyPrototype> Currency { get; set; } = "Thaler";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ulong SpecificMoneyCount { get; set; } = 0;

    [DataField, ViewVariables]
    public uint MaxOthers { get; set; } = 3;

    [ViewVariables]
    public List<NetEntity> Others { get; set; } = default!;
}

public enum StealMoneyReachType : byte
{
    DependsOnOthers, // When we can give the player list to other players and he should get money more than them
    SingleSpecificReach, // When we indicate specific count to reach
    AsPossible // He should reach much as possible money
}
