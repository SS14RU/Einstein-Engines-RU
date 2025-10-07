using Content.Server.Objectives;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Localization;
using Robust.Shared.Utility;

namespace Content.Server.AWS.CriminalAntag;

public sealed class StealMoneyConditionSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StealMoneyConditionComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<StealMoneyConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<StealMoneyConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnAssigned(Entity<StealMoneyConditionComponent> condition, ref ObjectiveAssignedEvent args)
    {
        condition.Comp.Others ??= new();

        if (condition.Comp.ReachType == StealMoneyReachType.SingleSpecificReach &&
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
        args.Progress = 1f;
    }
}
