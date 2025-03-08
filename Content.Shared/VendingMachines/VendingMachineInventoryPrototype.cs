using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.VendingMachines
{
    [Serializable, NetSerializable, Prototype("vendingMachineInventory")]
    public sealed partial class VendingMachineInventoryPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("startingInventory", customTypeSerializer:typeof(PrototypeIdDictionarySerializer<uint, EntityPrototype>))]
        public Dictionary<string, uint> StartingInventory { get; private set; } = new();

        [DataField("emaggedInventory", customTypeSerializer:typeof(PrototypeIdDictionarySerializer<uint, EntityPrototype>))]
        public Dictionary<string, uint>? EmaggedInventory { get; private set; }

        [DataField("contrabandInventory", customTypeSerializer:typeof(PrototypeIdDictionarySerializer<uint, EntityPrototype>))]
        public Dictionary<string, uint>? ContrabandInventory { get; private set; }
    }

    //SS14-RU
    [Serializable, NetSerializable, Prototype("vendingMachineInventoryPricing")]
    public sealed partial class VendingMachineInventoryPricingPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField(customTypeSerializer:typeof(PrototypeIdDictionarySerializer<ulong, EntityPrototype>))]
        public Dictionary<string, ulong> EntsPricing { get; private set; } = new();
    }
    //SS14-RU
}
