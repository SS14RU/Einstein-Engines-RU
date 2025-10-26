using Robust.Shared.Configuration;
using Content.Server.Voting.Managers;
using Content.Shared.GameTicking;
using Content.Shared.Voting;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Content.Server.GameTicking;

namespace Content.Server.AutoVote;

public sealed class AutoVoteSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IVoteManager _voteManager = default!;

    private const string MooseMapId = "Moose";
    private const string SecretPresetId = "secret";

    private bool _shouldVoteNextJoin;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnReturnedToLobby);
        SubscribeLocalEvent<PlayerJoinedLobbyEvent>(OnPlayerJoinedLobby);

        _cfgManager.OnValueChanged(CCVars.MapAutoVoteEnabled, EnsureMapAutoVoteDisabled, true);
        _cfgManager.OnValueChanged(CCVars.PresetAutoVoteEnabled, EnsurePresetAutoVoteDisabled, true);
        _cfgManager.OnValueChanged(CCVars.GameMap, EnsureMooseMap, true);
        _cfgManager.OnValueChanged(CCVars.GameLobbyDefaultPreset, EnsureSecretPreset, true);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfgManager.UnsubValueChanged(CCVars.MapAutoVoteEnabled, EnsureMapAutoVoteDisabled);
        _cfgManager.UnsubValueChanged(CCVars.PresetAutoVoteEnabled, EnsurePresetAutoVoteDisabled);
        _cfgManager.UnsubValueChanged(CCVars.GameMap, EnsureMooseMap);
        _cfgManager.UnsubValueChanged(CCVars.GameLobbyDefaultPreset, EnsureSecretPreset);
    }

    public void OnReturnedToLobby(RoundRestartCleanupEvent ev) => CallAutovote();

    public void OnPlayerJoinedLobby(PlayerJoinedLobbyEvent ev)
    {
        if (!_shouldVoteNextJoin)
            return;

        CallAutovote();
        _shouldVoteNextJoin = false;
    }

    private void CallAutovote()
    {
        if (_playerManager.PlayerCount == 0)
        {
            _shouldVoteNextJoin = true;
            return;
        }

        if (_cfgManager.GetCVar(CCVars.MapAutoVoteEnabled))
            _voteManager.CreateStandardVote(null, StandardVoteType.Map);
        if (_cfgManager.GetCVar(CCVars.PresetAutoVoteEnabled))
            _voteManager.CreateStandardVote(null, StandardVoteType.Preset);
    }

    private void EnsureMooseMap(string current)
    {
        if (current == MooseMapId)
            return;

        _cfgManager.SetCVar(CCVars.GameMap, MooseMapId);
    }

    private void EnsureSecretPreset(string current)
    {
        if (current == SecretPresetId)
            return;

        _cfgManager.SetCVar(CCVars.GameLobbyDefaultPreset, SecretPresetId);
    }

    private void EnsureMapAutoVoteDisabled(bool enabled)
    {
        if (!enabled)
            return;

        _cfgManager.SetCVar(CCVars.MapAutoVoteEnabled, false);
    }

    private void EnsurePresetAutoVoteDisabled(bool enabled)
    {
        if (!enabled)
            return;

        _cfgManager.SetCVar(CCVars.PresetAutoVoteEnabled, false);
    }
}
