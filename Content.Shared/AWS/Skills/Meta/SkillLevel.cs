using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Skills;

[Serializable, NetSerializable]
public enum SkillLevel : int
{
    NonSkilled,
    Basic,
    Trained,
    Experienced,
    Master,
}
