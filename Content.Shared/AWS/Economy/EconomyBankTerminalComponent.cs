using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
namespace Content.Shared.AWS.Economy
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
    public sealed partial class EconomyBankTerminalComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
        public ProtoId<CurrencyPrototype> AllowCurrency = "Thaler";

        [ViewVariables(VVAccess.ReadWrite), DataField]
        [AutoNetworkedField]
        public string LinkedAccount = "NO LINK TO ACCOUNT";

        [ViewVariables(VVAccess.ReadWrite)]
        [AutoNetworkedField]
        public ulong Amount = 0;

        [ViewVariables(VVAccess.ReadWrite)]
        [AutoNetworkedField]
        public string Reason = string.Empty;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public bool AllowUiEdit = false;
    }
}
