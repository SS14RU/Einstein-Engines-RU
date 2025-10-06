using System;
using Content.Server.Objectives;
using Content.Server.Shuttles.Systems;
using Content.Shared.Cuffs.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Localization;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Server.AWS.Economy.Bank;
using Content.Server.GameTicking;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;

namespace Content.Server.AWS.CriminalAntag;

public sealed class StealMoneyConditionSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly EconomyBankAccountSystem _economy = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private EntityQuery<StealMoneyConditionComponent> _stealMoneyQuery;

    public override void Initialize()
    {
        base.Initialize();

        _stealMoneyQuery = GetEntityQuery<StealMoneyConditionComponent>();

        SubscribeLocalEvent<StealMoneyConditionComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<StealMoneyConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<StealMoneyConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnAssigned(Entity<StealMoneyConditionComponent> condition, ref ObjectiveAssignedEvent args)
    {
        condition.Comp.Others ??= new();

        if ((StealMoneyReachType) condition.Comp.ReachType == StealMoneyReachType.SingleSpecificReach &&
            condition.Comp.SpecificMoneyCount == 0)
        {
            condition.Comp.SpecificMoneyCount = 500;
        }
    }

    private void OnAfterAssign(Entity<StealMoneyConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        var title = Loc.GetString("economy-criminalantag-objective-title");
        var description = Loc.GetString("economy-criminalantag-objective-desc");

        _metaData.SetEntityName(condition.Owner, title, args.Meta);
        _metaData.SetEntityDescription(condition.Owner, description, args.Meta);
        _objectives.SetIcon(condition.Owner, new SpriteSpecifier.Rsi(new ResPath("/Textures/AWS/economy/moneyholder.rsi"), "icon"), args.Objective);
    }

    private void OnGetProgress(Entity<StealMoneyConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        if (args.Mind.OwnedEntity is not { } uid || !EntityManager.EntityExists(uid))
            return;

        if (!HasEscaped(args.Mind))
        {
            args.Progress = 0f;
            return;
        }

        var progress = condition.Comp.ReachType switch
        {
            StealMoneyReachType.AsPossible => CalculateAsPossibleProgress(uid),
            StealMoneyReachType.SingleSpecificReach => CalculateSingleSpecificReachProgress(uid, condition.Comp),
            StealMoneyReachType.DependsOnOthers => 0f, // TODO: Implement when needed
            _ => throw new ArgumentOutOfRangeException(nameof(condition.Comp.ReachType),
                $"Unsupported reach type: {condition.Comp.ReachType}")
        };

        args.Progress = progress;
    }

    private float CalculateAsPossibleProgress(EntityUid uid)
    {
        if (_gameTicker.RunLevel != GameRunLevel.PostRound)
            return 0f;

        if (!EntityManager.EntityExists(uid))
            return 0f;

        var issuerMoney = _economy.CountHoldMoney(uid);
        var highestCompetitorMoney = FindHighestCompetitorMoney();

        return issuerMoney >= highestCompetitorMoney ? 1f : 0f;
    }

    private ulong FindHighestCompetitorMoney()
    {
        ulong maxMoney = 0;

        var query = EntityQueryEnumerator<MindComponent>();
        while (query.MoveNext(out var mindUid, out var mindComp))
        {
            if (mindComp.OwnedEntity is not { } owner)
                continue;

            foreach (var objective in mindComp.Objectives)
            {
                if (!TryComp<StealMoneyConditionComponent>(objective, out var stealMoneyCondition))
                    continue;

                if ((StealMoneyReachType) stealMoneyCondition.ReachType != StealMoneyReachType.AsPossible)
                    continue;

                var currentMoney = _economy.CountHoldMoney(owner);
                if (currentMoney > maxMoney)
                    maxMoney = currentMoney;
            }
        }

        return maxMoney;
    }

    private float CalculateSingleSpecificReachProgress(EntityUid uid, StealMoneyConditionComponent comp)
    {
        if (!EntityManager.EntityExists(uid))
            return 0f;

        return _economy.CountHoldMoney(uid);
    }

    private bool HasEscaped(MindComponent mind)
    {

        if (mind.OwnedEntity is not { } owned)
            return false;

        if (_mind.IsCharacterDeadIc(mind))
            return false;

        if (TryComp<CuffableComponent>(owned, out var cuffed) && cuffed.CuffedHandCount > 0)
            return false;

        return _emergencyShuttle.IsTargetEscaping(owned);
    }
}
