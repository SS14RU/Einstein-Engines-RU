using System;
using System.Collections.Generic;
using Content.Server.AWS.Economy.Bank;
using Content.Server.Objectives.Components;
using Content.Server.Roles;
using Content.Shared.AWS.CriminalAntag;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;

namespace Content.Server.AWS.CriminalAntag;

public readonly record struct CriminalAntagLeaderboardEntry(
    EntityUid MindId,
    MindComponent Mind,
    EntityUid? Body,
    string Name,
    ulong Money,
    bool Escaped);

public sealed class CriminalAntagLeaderboardSystem : EntitySystem
{
    [Dependency] private readonly EconomyBankAccountSystem _bank = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    public List<CriminalAntagLeaderboardEntry> CollectEntries()
    {
        var entries = new List<CriminalAntagLeaderboardEntry>();
        var minds = EntityQueryEnumerator<MindComponent>();
        while (minds.MoveNext(out var mindUid, out var mind))
        {
            if (mind.Deleted || !_roles.MindHasRole<CriminalAntagRoleComponent>(mindUid))
                continue;

            var body = ResolveTrackedBody(mind);
            var escaped = HasCompletedEscapeObjective(mindUid, mind);
            var money = escaped && body is { } entity ? _bank.CountHoldMoney(entity) : 0UL;
            var name = GetDisplayName(body, mind);

            entries.Add(new CriminalAntagLeaderboardEntry(mindUid, mind, body, name, money, escaped));
        }

        entries.Sort(static (a, b) => b.Money.CompareTo(a.Money));
        return entries;
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
