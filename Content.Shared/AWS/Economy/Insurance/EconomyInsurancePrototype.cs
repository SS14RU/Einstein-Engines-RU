using Robust.Shared.Utility;
using Robust.Shared.Prototypes;

namespace Content.Shared.AWS.Economy.Insurance;

[Prototype("insurance")]
public sealed partial class EconomyInsurancePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public string Name { get; private set; } = default!;

    [DataField(required: true)]
    public string Description { get; set; } = string.Empty;

    [DataField]
    public int Cost { get; set; } = 0;

    [DataField]
    public bool CanBeBought { get; set; } = true;

    // [DataField]
    // public Enum PayerType = EconomyInsurancePayerType.Character;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField]
    public int Priority { get; set; } = 0;
}
