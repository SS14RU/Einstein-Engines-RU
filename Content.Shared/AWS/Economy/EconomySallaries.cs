using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.AWS.Economy
{
    [Prototype("economySallaries")]
    public sealed partial class EconomySallariesPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public Dictionary<ProtoId<JobPrototype>, EconomySallariesJobEntry> Jobs = new();
    }

    [DataDefinition, Serializable]
    public partial struct EconomySallariesJobEntry
    {
        [ViewVariables(VVAccess.ReadWrite), DataField]
        public ulong StartMoney { get; set; } = 0;
        [ViewVariables(VVAccess.ReadWrite), DataField]
        public ulong Sallary { get; set; } = 0;
    }
}
