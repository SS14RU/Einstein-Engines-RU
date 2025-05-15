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