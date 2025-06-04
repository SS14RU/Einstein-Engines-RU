using Content.Server.Objectives.Components;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Server.AWS.Economy.Bank;
using Content.Server.GameTicking;
using System.Linq;

namespace Content.Server.AWS.CriminalAntag;

public sealed class StealMoneyConditionSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly EconomyBankAccountSystem _economy = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

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
        args.Cancelled = true;
        return;
    }

    private void OnAfterAssign(Entity<StealMoneyConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        _metaData.SetEntityName(condition.Owner, "title", args.Meta);
        _metaData.SetEntityDescription(condition.Owner, "", args.Meta);
        _objectives.SetIcon(condition.Owner, null!, args.Objective);
    }
    private void OnGetProgress(Entity<StealMoneyConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        if (args.Mind.OwnedEntity is not { } uid)
            return;

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
        return _economy.CountHoldMoney(uid);
    }
}
