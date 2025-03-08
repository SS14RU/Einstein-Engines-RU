using Robust.Shared.Prototypes;

namespace Content.Shared.AWS.Economy
{
    [Prototype("economyAccountId")]
    public sealed partial class EconomyAccountIdPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField(required: false)]
        public string Prefix = "";
        [DataField(required: false)]
        public string? Descriptior;
        [DataField(required: false)]
        public uint Streak = 4;
        [DataField(required: false)]
        public uint NumbersPerStreak = 4;

        [DataField(required: false)]
        public uint[] MinMaxSallary = {0,0};
        [DataField(required: false)]
        public uint[] MinMaxStartMoney = {0,0};
    }
}
