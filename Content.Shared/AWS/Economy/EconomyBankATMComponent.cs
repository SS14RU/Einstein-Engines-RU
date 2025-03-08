using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.AWS.Economy
{
    [RegisterComponent]
    public sealed partial class EconomyBankATMComponent : Component
    {
        public const string ATMCardId = "ATM-CardId";

        [DataField]
        public ItemSlot CardSlot = new();

        [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
        public EntProtoId<EconomyMoneyHolderComponent> MoneyHolderEntId = "ThalerHolder";

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public UInt16 EmagDropMoneyHolderRandomCount = 3;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public List<ulong> EmagDropMoneyValues = new();

        [DataField]
        public SoundSpecifier EmagSound = new SoundCollectionSpecifier("sparks");
    }
}
