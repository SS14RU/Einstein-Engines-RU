using Robust.Shared.GameStates;

namespace Content.Server.AWS.Economy.Salary;

[RegisterComponent, Access(typeof(SalaryDecreaseRuleSystem))]
public sealed partial class SalaryDecreaseRuleComponent : SalaryAdjustmentRuleComponent
{
    public SalaryDecreaseRuleComponent()
    {
        AnnouncementMessage = "salary-decrease-announcement";
    }

    public override bool Increase => false;
}
