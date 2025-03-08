using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.AWS.Economy;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EconomyAccountHolderComponent : Component
{
    public const string NoValueID = "NO VALUE";

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    public ProtoId<EconomyAccountIdPrototype> AccountIdByProto = "Nanotrasen";

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    public EntProtoId<EconomyMoneyHolderComponent> MoneyHolderEntId = "ThalerHolder";

    /// <summary>
    /// Set this up in <see cref="EconomyBankAccountSetup"></cref> to define the account, which this card will be using (referring to).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public string AccountID = "NO VALUE";

    /// <summary>
    /// Set this up in <see cref="EconomyBankAccountSetup"></cref> to define the name, which this card will be using.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public string AccountName = "UNEXPECTED USER";

    /// <summary>
    /// Use this in prototypes for defining the account, which this card will be using (the account will be initialized on spawn).
    /// Also, parameters beyond AccountName can be used with IDCards (if you want to setup other currency, for example).
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public BankAccountSetup? AccountSetup;
}
