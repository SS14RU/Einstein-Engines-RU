using Robust.Shared.Prototypes;

namespace Content.Shared.AWS.Economy.Insurance;

[RegisterComponent]
public sealed partial class EconomyInsuranceServerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public List<EconomyInsuranceInfo> InsuranceInfo = new();
}

public sealed class EconomyInsuranceInfo(ProtoId<EconomyInsurancePrototype> insuranceProto, string insurerName, string payerAccountId)
{
    public ProtoId<EconomyInsurancePrototype> InsuranceProto = insuranceProto;
    public string InsurerName = insurerName;
    public string PayerAccountId = payerAccountId;
}