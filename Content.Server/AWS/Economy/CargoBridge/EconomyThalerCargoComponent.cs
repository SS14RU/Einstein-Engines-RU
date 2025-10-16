using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server.AWS.Economy.CargoBridge;

/// <summary>
/// Maps a station to an AWS bank account that should back its cargo operations.
/// </summary>
[RegisterComponent]
public sealed partial class EconomyThalerCargoComponent : Component
{
    [DataField(required: true)]
    public string AccountId = string.Empty;

    [DataField]
    public ProtoId<CurrencyPrototype> Currency = "Thaler";
}
