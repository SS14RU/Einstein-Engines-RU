using System;
using System.Collections.Generic;
using System.Text;
using Content.Server.AWS.Economy.Bank;
using Content.Server.GameTicking;
using Content.Server.Objectives.Components;
using Content.Server.Roles;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;

namespace Content.Server.AWS.CriminalAntag;

/// <summary>
///     Adds a round-end leaderboard for criminal antagonists based on the thalers
///     they are holding when the round finishes.
/// </summary>
public sealed class CriminalAntagRoundEndSystem : EntitySystem
{
    [Dependency] private readonly EconomyBankAccountSystem _bank = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        var entries = new List<(string Name, ulong Money)>();
        var minds = EntityQueryEnumerator<MindComponent>();
        while (minds.MoveNext(out var mindUid, out var mind))
        {
            if (!_roles.MindHasRole<CriminalAntagRoleComponent>(mindUid))
                continue;

            var body = ResolveTrackedBody(mind);
            var escaped = HasCompletedEscapeObjective(mindUid, mind);

            var money = escaped && body is { } entity ? _bank.CountHoldMoney(entity) : 0UL;

            var name = GetDisplayName(body, mind);

            entries.Add((name, money));
        }

        if (entries.Count == 0)
            return;

        entries.Sort(static (a, b) =>
        {
            var money = b.Money.CompareTo(a.Money);
            if (money != 0)
                return money;

            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        });

        var text = new StringBuilder();
        text.AppendLine(Loc.GetString("economy-criminalantag-round-end-header"));

        for (var i = 0; i < entries.Count; i++)
        {
            var (name, money) = entries[i];
            text.AppendLine(Loc.GetString(
                "economy-criminalantag-round-end-entry",
                ("index", i + 1),
                ("name", name),
                ("money", money)));
        }

        ev.AddLine(text.ToString());
    }

    private bool HasCompletedEscapeObjective(EntityUid mindUid, MindComponent mind)
    {
        foreach (var objective in mind.Objectives)
        {
            if (!HasComp<EscapeShuttleConditionComponent>(objective))
                continue;

            return _objectives.IsCompleted(objective, (mindUid, mind));
        }

        return false;
    }

    private string GetDisplayName(EntityUid? body, MindComponent mind)
    {
        if (body is { } namedEntity && EntityManager.EntityExists(namedEntity))
        {
            var name = Identity.Name(namedEntity, EntityManager);
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        if (!string.IsNullOrWhiteSpace(mind.CharacterName))
            return mind.CharacterName!;

        return Loc.GetString("economy-criminalantag-round-end-unknown");
    }

    private EntityUid? ResolveTrackedBody(MindComponent mind)
    {
        var owner = mind.OwnedEntity;
        if (owner is not null && EntityManager.EntityExists(owner.Value))
            return owner;

        if (mind.OriginalOwnedEntity != null &&
            TryGetEntity(mind.OriginalOwnedEntity, out var original) &&
            EntityManager.EntityExists(original))
        {
            return original;
        }

        return null;
    }
}
