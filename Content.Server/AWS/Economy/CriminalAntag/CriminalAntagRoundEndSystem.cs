using System.Text;
using Content.Server.GameTicking;
using Robust.Shared.Localization;

namespace Content.Server.AWS.CriminalAntag;

/// <summary>
///     Adds a round-end leaderboard for criminal antagonists based on the thalers
///     they are holding when the round finishes.
/// </summary>
public sealed class CriminalAntagRoundEndSystem : EntitySystem
{
    [Dependency] private readonly CriminalAntagLeaderboardSystem _leaderboard = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        var entries = _leaderboard.CollectEntries();
        if (entries.Count == 0)
            return;

        var text = new StringBuilder();
        text.AppendLine(Loc.GetString("economy-criminalantag-round-end-header"));

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            text.AppendLine(Loc.GetString(
                "economy-criminalantag-round-end-entry",
                ("index", i + 1),
                ("name", entry.Name),
                ("money", entry.Money)));
        }

        ev.AddLine(text.ToString());
    }
}
