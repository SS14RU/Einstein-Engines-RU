using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.AWS.Gravity;

/// <summary>
///     Networked status info for gravity generators so clients can pull station mass/FTL state without touching base power systems.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
public sealed partial class AwsGravityGeneratorStatusComponent : Component
{
    [AutoNetworkedField]
    public bool ShowStatus;

    [AutoNetworkedField]
    public float StationMass;

    [AutoNetworkedField]
    public bool StationFtlLocked;
}
