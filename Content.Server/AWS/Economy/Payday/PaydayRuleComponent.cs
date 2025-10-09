using System;
using System.Collections.Generic;
using Content.Shared.AWS.Economy.Bank;
using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.AWS.Economy.Payday;

[RegisterComponent, Access(typeof(PaydayRuleSystem)), AutoGenerateComponentPause]
public sealed partial class PaydayRuleComponent : Component
{
    /// <summary>
    /// Interval between salary payouts, measured in minutes.
    /// </summary>
    [DataField]
    public float IntervalMinutes = 10f;

    /// <summary>
    /// Salaries prototype used when we need to fall back and determine the base salary for a job.
    /// </summary>
    [DataField]
    public ProtoId<EconomySallariesPrototype> SalaryPrototype = "NanotrasenDefaultSallaries";

    /// <summary>
    /// When no department account can be matched, this account will be used as a fallback payer.
    /// </summary>
    [DataField]
    public string FallbackAccountId = "NT-CentCom";

    /// <summary>
    /// Locale id that will be announced to the crew after a payout is processed.
    /// </summary>
    [DataField]
    public string AnnouncementMessage = "payday-rule-announcement";

    /// <summary>
    /// The announcer prototype id to use when broadcasting the announcement.
    /// </summary>
    [DataField]
    public string AnnouncementId = "PaydayRuleAnnouncement";

    /// <summary>
    /// Popup message that will be sent to crew members whose salary could not be paid.
    /// </summary>
    [DataField]
    public string FailurePopup = "payday-rule-popup-insufficient";

    /// <summary>
    /// Optional overrides that force specific jobs to use a specific payer account.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, string> JobAccountOverrides = new();

    /// <summary>
    /// The next time when salaries should be paid.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? NextPayoutAt;

    public TimeSpan Interval => TimeSpan.FromMinutes(Math.Max(0.01f, IntervalMinutes));

    /// <summary>
    /// Cache salary entries to avoid prototype lookups for every account.
    /// </summary>
    public readonly Dictionary<ProtoId<JobPrototype>, EconomySallariesJobEntry> SalaryCache = new();
}
