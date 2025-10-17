using Robust.Shared.Localization;
using Robust.Shared.Prototypes;

namespace Content.Shared.AWS.Economy.Bank.Prototypes;

[Prototype("economyAccountPrefix")]
public sealed class EconomyBankAccountPrefixPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("displayLoc")]
    public LocId? DisplayLoc;

    [DataField("display")]
    public string? Display;

    [DataField("prefix")]
    public string Prefix { get; private set; } = string.Empty;

    [DataField("order")]
    public int Order { get; private set; }

    [DataField("isDefault")]
    public bool IsDefault { get; private set; }
}
