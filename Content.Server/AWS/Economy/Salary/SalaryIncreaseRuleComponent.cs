using Robust.Shared.GameStates;

namespace Content.Server.AWS.Economy.Salary;

[RegisterComponent, Access(typeof(SalaryIncreaseRuleSystem))]
public sealed partial class SalaryIncreaseRuleComponent : SalaryAdjustmentRuleComponent
{
    public SalaryIncreaseRuleComponent()
    {
        AnnouncementMessage = "salary-increase-announcement";
    }

    public override bool Increase => true;
}
