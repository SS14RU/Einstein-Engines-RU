using Robust.Shared.Prototypes;
using Content.Shared.Humanoid.Prototypes;

namespace Content.Shared.AWS.Skills;

[Prototype("ageSkillPoints")]
public sealed partial class AgeSkillPointsPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    public ProtoId<SpeciesPrototype> Specie = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    public Dictionary<int, float> PointsForAges = new();

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int MinAge = 18;
}
