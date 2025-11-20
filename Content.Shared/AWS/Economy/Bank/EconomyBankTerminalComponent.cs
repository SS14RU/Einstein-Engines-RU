using Content.Shared.Store;
using Content.Shared.VendingMachines;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.AWS.Economy.Bank
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

        /// <summary>
        ///     Stores data about the selected vending item while the terminal processes the purchase.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public string? PendingItemId;

        [ViewVariables(VVAccess.ReadWrite)]
        public InventoryType PendingInventoryType = InventoryType.Regular;
    }
}
