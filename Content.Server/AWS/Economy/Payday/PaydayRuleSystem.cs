using System;
using System.Collections.Generic;
using System.Linq;
using Content.Server.Announcements.Systems;
using Content.Server.AWS.Economy.Bank;
using Content.Server.GameTicking.Rules;
using Content.Server.Popups;
using Content.Shared.AWS.Economy.Bank;
using Content.Shared.GameTicking.Components;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Prototypes;

namespace Content.Server.AWS.Economy.Payday;

public sealed class PaydayRuleSystem : GameRuleSystem<PaydayRuleComponent>
{
    [Dependency] private readonly EconomyBankAccountSystem _bank = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PaydayRuleComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentShutdown(EntityUid uid, PaydayRuleComponent component, ComponentShutdown args)
    {
        component.NextPayoutAt = null;
    }

    protected override void Started(EntityUid uid, PaydayRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        component.NextPayoutAt = Timing.CurTime;
    }

    protected override void Ended(EntityUid uid, PaydayRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);
        component.NextPayoutAt = null;
    }

    protected override void ActiveTick(EntityUid uid, PaydayRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.NextPayoutAt is null)
            component.NextPayoutAt = Timing.CurTime;

        if (Timing.CurTime < component.NextPayoutAt)
            return;

        ProcessPayday(component);
        component.NextPayoutAt = Timing.CurTime + component.Interval;
    }

    private void ProcessPayday(PaydayRuleComponent component)
    {
        var payerCache = new Dictionary<string, Entity<EconomyBankAccountComponent>>();
        var accounts = _bank.GetAccounts(EconomyBankAccountMask.ByTags, new List<BankAccountTag> { BankAccountTag.Personal });
        var reason = Loc.GetString("economybanksystem-log-reason-payday");

        var paidAnyone = false;

        foreach (var accountEntity in OrderAccountsBySalary(accounts.Values))
        {
            var account = accountEntity.Comp;

            if (account.Blocked || !account.CanReachPayDay)
                continue;

            EconomySallariesJobEntry? salaryEntry = null;
            if (account.JobName is { } jobId)
                _bank.TryGetSalaryJobEntry(jobId, component.SalaryPrototype, out salaryEntry);

            if (account.Salary is not { } salary || salary == 0)
            {
                if (salaryEntry is null)
                    continue;

                var baseSalary = salaryEntry.Value.Sallary;
                var adjusted = Math.Round(baseSalary * _bank.SalaryMultiplier);
                adjusted = Math.Clamp(adjusted, 0d, ulong.MaxValue);
                salary = (ulong) adjusted;
                if (salary == 0)
                    continue;
            }

            var payerAccountId = ResolvePayerAccount(component, account.JobName, salaryEntry);
            if (payerAccountId is null)
                continue;

            if (!TryGetPayerAccount(payerAccountId, payerCache, out var payerAccount))
                continue;

            if (payerAccount.Comp.Balance < salary)
            {
                NotifyInsufficientFunds(component, account.AccountID);
                continue;
            }

            if (!_bank.TrySendMoney(payerAccountId, account.AccountID, salary, reason, out _))
            {
                NotifyInsufficientFunds(component, account.AccountID);
                continue;
            }

            paidAnyone = true;
        }

        if (paidAnyone)
            _announcer.SendAnnouncement(component.AnnouncementId, component.AnnouncementMessage);

        RaiseLocalEvent(new EconomySallaryPostEvent());
    }

    private void NotifyInsufficientFunds(PaydayRuleComponent component, string accountId)
    {
        var owner = FindAccountOwner(accountId);
        if (owner is null)
            return;

        _popup.PopupEntity(Loc.GetString(component.FailurePopup), owner.Value, owner.Value);
    }

    private bool TryGetPayerAccount(string accountId, Dictionary<string, Entity<EconomyBankAccountComponent>> cache, out Entity<EconomyBankAccountComponent> account)
    {
        if (cache.TryGetValue(accountId, out account))
            return true;

        if (!_bank.TryGetAccount(accountId, out var accountEntity))
        {
            account = default;
            return false;
        }

        account = accountEntity.Value;
        cache[accountId] = account;
        return true;
    }

    private EntityUid? FindAccountOwner(string accountId)
    {
        var query = EntityQueryEnumerator<EconomyAccountHolderComponent>();
        while (query.MoveNext(out var uid, out var holder))
        {
            if (holder.AccountID == accountId)
                return uid;
        }

        return null;
    }

    private string? ResolvePayerAccount(PaydayRuleComponent component, ProtoId<JobPrototype>? job, EconomySallariesJobEntry? salaryEntry)
    {
        if (job is { } jobId && component.JobAccountOverrides.TryGetValue(jobId, out var overrideAccount))
            return overrideAccount;

        if (salaryEntry is { AccountId: { } accountId } && !string.IsNullOrEmpty(accountId))
            return accountId;

        return string.IsNullOrEmpty(component.FallbackAccountId) ? null : component.FallbackAccountId;
    }

    private static IEnumerable<Entity<EconomyBankAccountComponent>> OrderAccountsBySalary(IEnumerable<Entity<EconomyBankAccountComponent>> accounts)
    {
        return accounts
            .OrderByDescending(static account => account.Comp.Salary ?? 0UL)
            .ThenByDescending(static account => account.Comp.JobName?.Id ?? string.Empty);
    }
}
