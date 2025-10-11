using System;
using System.Collections.Generic;
using Content.Server.Administration;
using Content.Server.AWS.Economy.Payday;
using Content.Server.AWS.Economy.Salary;
using Content.Server.GameTicking;
using Content.Shared.Administration;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Server.AWS.Economy.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed class EconomyRuleCommand : IConsoleCommand
{
    private static readonly string[] Actions = ["start", "stop"];
    private static readonly string[] EconomyRules =
    [
        "PaydayRule",
        "SalaryIncreaseRule",
        "SalaryDecreaseRule"
    ];

    public string Command => "economyAddRule";
    public string Description => "Controls economy-related game rules (start / stop).";
    public string Help => "Usage:\n" +
                          "  economyAddRule start <RuleId> [percent]\n" +
                          "  economyAddRule stop <RuleId>\n" +
                          "Examples:\n" +
                          "  economyAddRule start PaydayRule\n" +
                          "  economyAddRule start SalaryIncreaseRule 25\n" +
                          "  economyAddRule stop PaydayRule";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError("Not enough arguments. Use 'economyAddRule start <RuleId>' or 'economyAddRule stop <RuleId>'.");
            return;
        }

        var action = args[0].ToLowerInvariant();
        var ruleId = args[1];

        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var systemManager = IoCManager.Resolve<IEntitySystemManager>();
        var ticker = systemManager.GetEntitySystem<GameTicker>();

        switch (action)
        {
            case "start":
                StartRule(shell, prototypeManager, entityManager, ticker, ruleId, args);
                break;

            case "stop":
                StopRule(shell, entityManager, ticker, ruleId);
                break;

            default:
                shell.WriteError($"Unknown action '{args[0]}'. Expected 'start' or 'stop'.");
                break;
        }
    }

    private void StartRule(IConsoleShell shell,
        IPrototypeManager prototypeManager,
        IEntityManager entityManager,
        GameTicker ticker,
        string ruleId,
        IReadOnlyList<string> args)
    {
        if (!prototypeManager.HasIndex<EntityPrototype>(ruleId))
        {
            shell.WriteError($"Unknown rule '{ruleId}'.");
            return;
        }

        var ruleEntity = ticker.AddGameRule(ruleId);

        if (entityManager.TryGetComponent(ruleEntity, out SalaryAdjustmentRuleComponent? salaryComponent) &&
            args.Count >= 3 &&
            int.TryParse(args[2], out var overridePercent))
        {
            salaryComponent.OverridePercent = overridePercent;
            entityManager.Dirty(ruleEntity, salaryComponent);
        }

        if (ticker.RunLevel == GameRunLevel.InRound && ticker.StartGameRule(ruleEntity))
            shell.WriteLine($"Started rule '{ruleId}'.");
        else
            shell.WriteLine($"Added rule '{ruleId}'. It will start automatically when the round begins.");
    }

    private void StopRule(IConsoleShell shell,
        IEntityManager entityManager,
        GameTicker ticker,
        string ruleId)
    {
        var stopped = 0;
        var query = entityManager.EntityQueryEnumerator<GameRuleComponent>();
        while (query.MoveNext(out var uid, out var gameRule))
        {
            if (!ticker.IsGameRuleActive(uid, gameRule))
                continue;

            if (!TryMatchRule(entityManager, uid, ruleId))
                continue;

            if (ticker.EndGameRule(uid, gameRule))
                stopped++;
        }

        shell.WriteLine(stopped == 0
            ? $"No active rules matching '{ruleId}' were found."
            : $"Stopped {stopped} active rule(s) matching '{ruleId}'.");
    }

    private bool TryMatchRule(IEntityManager entityManager, EntityUid uid, string expectedId)
    {
        if (!entityManager.TryGetComponent(uid, out MetaDataComponent? meta))
            return false;

        var prototypeId = meta.EntityPrototype?.ID;
        return prototypeId == expectedId;
    }

    public CompletionResult GetCompletions(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(Actions, "<start|stop>"),
            2 when args[0].Equals("start", StringComparison.OrdinalIgnoreCase)
                 || args[0].Equals("stop", StringComparison.OrdinalIgnoreCase)
                => CompletionResult.FromHintOptions(EconomyRules, "<rule>"),
            3 when args[0].Equals("start", StringComparison.OrdinalIgnoreCase)
                 && (args[1].Equals("SalaryIncreaseRule", StringComparison.OrdinalIgnoreCase)
                     || args[1].Equals("SalaryDecreaseRule", StringComparison.OrdinalIgnoreCase))
                => CompletionResult.FromHint("optional percent (no bounds)"),
            _ => CompletionResult.Empty
        };
    }
}
