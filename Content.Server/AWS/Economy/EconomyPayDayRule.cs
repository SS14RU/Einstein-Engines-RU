using Content.Shared.GameTicking.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.AWS.Economy;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Mind.Components;
using Content.Shared.Roles.Jobs;
using Content.Server.AWS.Economy;

namespace Content.Server.StationEvents.Events;

public sealed class EconomyPayDayRule : StationEventSystem<EconomyPayDayRuleComponent>
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly EconomyBankAccountSystem _bankAccountSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Started(EntityUid uid, EconomyPayDayRuleComponent ruleComponent, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        var accounts = _bankAccountSystem.GetAccounts();
        if (!_bankAccountSystem.TryGetAccount(ruleComponent.PayerAccountId, out var payerAccount))
            return;

        if (!_prototype.TryIndex(ruleComponent.SallaryProto, out var sallariesProto))
            return;

        var enumerator = _entMan.AllEntityQueryEnumerator<MindContainerComponent>();
        Dictionary<string, ProtoId<JobPrototype>> manifest = new();
        while(enumerator.MoveNext(out var ent, out var mindContainerComponent))
        {
            if (TryComp<MindComponent>(mindContainerComponent.Mind, out var mindComponent)
                && TryComp<JobRoleComponent>(mindContainerComponent.Mind, out var jobComponent)
                && mindComponent.CharacterName is not null
                && jobComponent.Prototype is not null)
            {
                manifest.Add(mindComponent.CharacterName, jobComponent.Prototype.Value);
            }
        }
        List<Entity<EconomyBankAccountComponent>> blockedAccounts = new();

        foreach (var (_, accountEntity) in accounts)
        {
            var account = accountEntity.Comp;

            if (account.Blocked || !account.CanReachPayDay)
                continue;

            if (account.Blocked)
                continue;

            if (!manifest.TryGetValue(account.AccountName, out var job))
                continue;

            EconomySallariesJobEntry? entry = null;

            foreach (var item in sallariesProto.Jobs)
            {
                if (item.Key.Id == job.Id)
                    entry = item.Value;
            }

            if (entry is null)
                continue;

            ulong sallary = ((ulong)ruleComponent.Coef.Next(_random))/100* entry.Value.Sallary;
            string? err;
            var reason = Loc.GetString("economybanksystem-log-reason-payday");

            switch (ruleComponent.PayType)
            {
                case EconomyPayDayRuleType.Adding:
                    _bankAccountSystem.TrySendMoney(payerAccount.Comp.AccountID, account.AccountID, sallary, reason, out err);
                    break;
                case EconomyPayDayRuleType.Decrementing:
                    if (!_bankAccountSystem.TrySendMoney(account.AccountID, payerAccount.Comp.AccountID, sallary, reason, out err))
                    {
                        if (_bankAccountSystem.TrySetAccountParameter(account.AccountID, EconomyBankAccountParam.Blocked, true))
                            blockedAccounts.Add(accountEntity);
                    }
                    break;
                default:
                    break;
            }
        }

        //notify that we blocked, or we cant proccess any payment
    }
}
