using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Skills;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class SkillContainer
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public Dictionary<ProtoId<SkillPrototype>, Enum> Skills = new();

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public Dictionary<ProtoId<SkillPrototype>, List<Enum>> UnblockedSkillLevels = new();

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public Dictionary<ProtoId<SkillPrototype>, List<Enum>> BlockedSkillLevels = new();

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public Dictionary<ProtoId<SkillPrototype>, Enum> DefaultSkillLevels = new();

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int AdditionalSkillPoints { get; set; } = 0;
}
