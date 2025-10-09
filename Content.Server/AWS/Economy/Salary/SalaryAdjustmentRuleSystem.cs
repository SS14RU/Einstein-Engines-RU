using System;
using System.Collections.Generic;
using Content.Server.Announcements.Systems;
using Content.Server.AWS.Economy.Bank;
using Content.Server.GameTicking.Rules;
using Content.Shared.AWS.Economy.Bank;
using Content.Shared.GameTicking.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Random;

namespace Content.Server.AWS.Economy.Salary;

public abstract class SalaryAdjustmentRuleSystem<TComponent> : GameRuleSystem<TComponent>
    where TComponent : SalaryAdjustmentRuleComponent
{
    [Dependency] private readonly EconomyBankAccountSystem _bank = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!;

    protected override void Started(EntityUid uid, TComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var percent = ResolvePercent(component);
        component.AppliedPercent = percent;

        var multiplier = CalculateMultiplier(component, percent);
        _bank.ApplySalaryMultiplier(multiplier);

        if (!string.IsNullOrEmpty(component.AnnouncementMessage))
        {
            _announcer.SendAnnouncement(
                component.AnnouncementId,
                component.AnnouncementMessage,
                localeArgs: ("percent", percent));
        }

        GameTicker.EndGameRule(uid, gameRule);
    }

    private static double CalculateMultiplier(SalaryAdjustmentRuleComponent component, int percent)
    {
        var ratio = percent / 100.0;
        return component.Increase ? 1.0 + ratio : 1.0 - ratio;
    }

    private int ResolvePercent(SalaryAdjustmentRuleComponent component)
    {
        if (component.OverridePercent is { } overridePercent)
            return overridePercent;

        var values = new List<int>();
        var step = Math.Max(1, component.Step);

        for (var value = component.MinPercent; value <= component.MaxPercent; value += step)
            values.Add(value);

        if (values.Count == 0)
            values.Add(component.MinPercent);

        return RobustRandom.Pick(values);
    }
}
