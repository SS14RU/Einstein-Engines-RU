using Robust.Shared.Prototypes;

namespace Content.Server.AWS.Economy.Salary;

public abstract partial class SalaryAdjustmentRuleComponent : Component
{
    [DataField]
    public int MinPercent = 10;

    [DataField]
    public int MaxPercent = 60;

    [DataField]
    public int Step = 5;

    [DataField]
    public int? OverridePercent;

    [DataField]
    public string AnnouncementMessage = string.Empty;

    [DataField]
    public string AnnouncementId = "PaydayRuleAnnouncement";

    [DataField]
    public int? AppliedPercent;

    public abstract bool Increase { get; }
}
