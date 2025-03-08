using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
namespace Content.Shared.AWS.Economy
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
    public sealed partial class EconomyMoneyHolderComponent : Component, IEconomyMoneyHolder
    {
        [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
        public ProtoId<CurrencyPrototype> AllowCurrency;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        [AutoNetworkedField]
        public ulong Balance { get; set; } = 0;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        [AutoNetworkedField]
        public bool Emagged = false;
    }
}
