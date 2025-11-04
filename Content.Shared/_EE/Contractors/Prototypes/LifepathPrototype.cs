using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Customization.Systems;
using Content.Shared.Traits;
using Content.Shared.Roles;


namespace Content.Shared._EE.Contractors.Prototypes;

/// <summary>
/// Prototype representing a character's Lifepath in YAML.
/// </summary>
[Prototype("lifepath")]
public sealed partial class LifepathPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; } = string.Empty;

    [DataField]
    public string NameKey { get; } = string.Empty;

    [DataField]
    public string DescriptionKey { get; } = string.Empty;

    [DataField]
    public List<CharacterRequirement> Requirements = new();

    [DataField]
    public List<ProtoId<JobPrototype>> BlockingJobs { get; } = new();

    [DataField(serverOnly: true)]
    public TraitFunction[] Functions { get; private set; } = Array.Empty<TraitFunction>();
}
