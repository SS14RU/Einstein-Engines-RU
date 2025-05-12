using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy.Insurance;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class EconomyInsuranceServerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public List<EconomyInsuranceInfo> InsuranceInfo { get; set; } = new();
}

[Serializable, NetSerializable]
public sealed class EconomyInsuranceInfo(ProtoId<EconomyInsurancePrototype> insuranceProto, string insurerName, string payerAccountId, string dna)
{
    public ProtoId<EconomyInsurancePrototype> InsuranceProto { get; set; } = insuranceProto;
    public string InsurerName { get; set; } = insurerName;
    public string PayerAccountId { get; set; } = payerAccountId;
    public string DNA { get; private set; } = dna;
}
