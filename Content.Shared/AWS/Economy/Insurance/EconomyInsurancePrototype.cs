using Robust.Shared.Utility;
using Robust.Shared.Prototypes;

namespace Content.Shared.AWS.Economy.Insurance;

[Prototype("insurance")]
public sealed partial class EconomyInsurancePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name { get; private set; } = default!;

    [DataField]
    public string Description = string.Empty;

    [DataField(required: true)]
    public int Cost = 0;

    [DataField]
    public bool CanBeBought = true;

    // [DataField]
    // public Enum PayerType = EconomyInsurancePayerType.Character;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField]
    public int Priority = 0;
}