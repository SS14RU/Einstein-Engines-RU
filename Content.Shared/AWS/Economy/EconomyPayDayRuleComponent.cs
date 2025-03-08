using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Content.Shared.Roles;
using Content.Shared.Destructible.Thresholds;
namespace Content.Shared.AWS.Economy
{
    [RegisterComponent]
    public sealed partial class EconomyPayDayRuleComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
        public string PayerAccountId;

        [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
        public string ProccessOnlyPrefixedAccounts;

        [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
        public Enum PayType = EconomyPayDayRuleType.Adding;

        [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
        public ProtoId<EconomySallariesPrototype> SallaryProto;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public MinMax Coef;

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public List<ProtoId<JobPrototype>> IncludedJobs = new();

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public List<ProtoId<DepartmentPrototype>> IncludedDepartments = new ();

        [ViewVariables(VVAccess.ReadWrite), DataField]
        public Enum IncludeFlag = EconomyPayDayRuleOnlyPayFor.OnlyIncludeJobs;
    }

    [Serializable]
    public enum EconomyPayDayRuleType
    {
        Adding,
        Decrementing,
    }

    [Serializable]
    public enum EconomyPayDayRuleOnlyPayFor
    {
        OnlyIncludeJobs,
        OnlyIncludeDepartments,
        IncludeJobsDepartments
    }
}
