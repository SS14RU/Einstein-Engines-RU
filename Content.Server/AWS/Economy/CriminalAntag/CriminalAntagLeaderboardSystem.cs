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
            if (!TryBuildEntry(mindUid, mind, out var entry))
                continue;

            entries.Add(entry);
        }

        entries.Sort(static (a, b) => b.Money.CompareTo(a.Money));
        return entries;
    }

    public bool HasTopMoney(EntityUid mindUid, MindComponent mind)
    {
        if (!TryGetCriminalFinancials(mindUid, mind, requireEscape: false, out _, out var subjectMoney, out _))
            return false;

        if (subjectMoney == 0)
            return false;

        var minds = EntityQueryEnumerator<MindComponent>();
        while (minds.MoveNext(out var otherUid, out var otherMind))
        {
            if (otherUid == mindUid)
                continue;

            if (!TryGetCriminalFinancials(otherUid, otherMind, requireEscape: false, out _, out var otherMoney, out _))
                continue;

            if (otherMoney == 0)
                continue;

            if (otherMoney > subjectMoney)
                return false;
        }

        return true;
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

    private bool TryBuildEntry(EntityUid mindUid, MindComponent mind, out CriminalAntagLeaderboardEntry entry)
    {
        entry = default;

        if (!TryGetCriminalFinancials(mindUid, mind, requireEscape: true, out var body, out var money, out var escaped))
            return false;

        entry = new CriminalAntagLeaderboardEntry(
            mindUid,
            mind,
            body,
            GetDisplayName(body, mind),
            money,
            escaped);

        return true;
    }

    private bool TryGetCriminalFinancials(EntityUid mindUid, MindComponent mind, bool requireEscape, out EntityUid? body, out ulong money, out bool escaped)
    {
        body = null;
        money = 0;
        escaped = false;

        if (mind.Deleted || !_roles.MindHasRole<CriminalAntagRoleComponent>(mindUid))
            return false;

        body = ResolveTrackedBody(mind);

        if (requireEscape)
        {
            escaped = HasCompletedEscapeObjective(mindUid, mind);
            if (!escaped || body is not { } escapedEntity)
                return true;

            money = _bank.CountHoldMoney(escapedEntity);
            return true;
        }

        if (body is { } entity)
            money = _bank.CountHoldMoney(entity);

        return true;
    }
}
