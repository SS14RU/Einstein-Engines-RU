using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy.Insurance;

[Serializable, NetSerializable]
public sealed class EconomyInsuranceInfo(ProtoId<EconomyInsurancePrototype> insuranceProto, string insurerName, string payerAccountId, string dna)
{
    public ProtoId<EconomyInsurancePrototype> InsuranceProto { get; set; } = insuranceProto;
    public string InsurerName { get; set; } = insurerName;
    public string PayerAccountId { get; set; } = payerAccountId;
    public string DNA { get; private set; } = dna;
}