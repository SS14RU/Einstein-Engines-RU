using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Access.Components;
using Content.Server.Access.Systems;
using Content.Server.DetailExaminable;
using Content.Server.GameTicking;
using Content.Server.Humanoid;
using Content.Server.IdentityManagement;
using Content.Server.Mind.Commands;
using Content.Server.PDA;
using Content.Server.Shuttles.Systems;
using Content.Server.Silicon.IPC;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Spawners.Components;
using Content.Server.Station.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Customization.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.PDA;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using Content.Shared.Station;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Server.AWS.Economy.Bank;
using Content.Shared.AWS.Economy.Bank;

namespace Content.Server.Station.Systems;

/// <summary>
/// Manages spawning into the game, tracking available spawn points.
/// Also provides helpers for spawning in the player's mob.
/// </summary>
[PublicAPI]
public sealed class StationSpawningSystem : SharedStationSpawningSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidSystem = default!;
    [Dependency] private readonly IdCardSystem _cardSystem = default!;
    [Dependency] private readonly PdaSystem _pdaSystem = default!;
    [Dependency] private readonly SharedAccessSystem _accessSystem = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly MetaDataSystem _metaSystem = default!;
    [Dependency] private readonly InternalEncryptionKeySpawner _internalEncryption = default!;
    [Dependency] private readonly EconomyBankAccountSystem _bankAccountSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private bool _randomizeCharacters;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_configurationManager, CCVars.ICRandomCharacters, e => _randomizeCharacters = e, true);
    }

    /// <summary>
    /// Attempts to spawn a player character onto the given station.
    /// </summary>
    /// <param name="station">Station to spawn onto.</param>
    /// <param name="job">The job to assign, if any.</param>
    /// <param name="profile">The character profile to use, if any.</param>
    /// <param name="stationSpawning">Resolve pattern, the station spawning component for the station.</param>
    /// <param name="spawnPointType">Delta-V: Set desired spawn point type.</param>
    /// <returns>The resulting player character, if any.</returns>
    /// <exception cref="ArgumentException">Thrown when the given station is not a station.</exception>
    /// <remarks>
    /// This only spawns the character, and does none of the mind-related setup you'd need for it to be playable.
    /// </remarks>
    public EntityUid? SpawnPlayerCharacterOnStation(EntityUid? station, ProtoId<JobPrototype>? job, HumanoidCharacterProfile? profile, StationSpawningComponent? stationSpawning = null, SpawnPointType spawnPointType = SpawnPointType.Unset)
    {
        if (station != null && !Resolve(station.Value, ref stationSpawning))
            throw new ArgumentException("Tried to use a non-station entity as a station!", nameof(station));

        var ev = new PlayerSpawningEvent(job, profile, station, spawnPointType);

        RaiseLocalEvent(ev);
        DebugTools.Assert(ev.SpawnResult is { Valid: true } or null);

        return ev.SpawnResult;
    }

    //TODO: Figure out if everything in the player spawning region belongs somewhere else.
    #region Player spawning helpers

    /// <summary>
    /// Spawns in a player's mob according to their job and character information at the given coordinates.
    /// Used by systems that need to handle spawning players.
    /// </summary>
    /// <param name="coordinates">Coordinates to spawn the character at.</param>
    /// <param name="job">Job to assign to the character, if any.</param>
    /// <param name="profile">Appearance profile to use for the character.</param>
    /// <param name="station">The station this player is being spawned on.</param>
    /// <param name="entity">The entity to use, if one already exists.</param>
    /// <returns>The spawned entity</returns>
     public EntityUid SpawnPlayerMob(
        EntityCoordinates coordinates,
        ProtoId<JobPrototype>? job,
        HumanoidCharacterProfile? profile,
        EntityUid? station,
        EntityUid? entity = null)
    {
        _prototypeManager.TryIndex(job ?? string.Empty, out var prototype);

        // If we're not spawning a humanoid, we're gonna exit early without doing all the humanoid stuff.
        if (prototype?.JobEntity != null)
        {
            DebugTools.Assert(entity is null);
            var jobEntity = EntityManager.SpawnEntity(prototype.JobEntity, coordinates);
            MakeSentientCommand.MakeSentient(jobEntity, EntityManager);
            DoJobSpecials(job, jobEntity);
            _identity.QueueIdentityUpdate(jobEntity);
            return jobEntity;
        }

        string speciesId;
        if (_randomizeCharacters)
        {
            var weightId = _configurationManager.GetCVar(CCVars.ICRandomSpeciesWeights);
            var weights = _prototypeManager.Index<WeightedRandomSpeciesPrototype>(weightId);
            speciesId = weights.Pick(_random);
        }
        else if (profile != null)
            speciesId = profile.Species;
        else
            speciesId = SharedHumanoidAppearanceSystem.DefaultSpecies;

        if (!_prototypeManager.TryIndex<SpeciesPrototype>(speciesId, out var species))
            throw new ArgumentException($"Invalid species prototype was used: {speciesId}");

        entity ??= Spawn(species.Prototype, coordinates);

        if (_randomizeCharacters)
            profile = HumanoidCharacterProfile.RandomWithSpecies(speciesId);

        if (prototype?.StartingGear != null)
        {
            var startingGear = _prototypeManager.Index<StartingGearPrototype>(prototype.StartingGear);
            if (profile != null)
                startingGear = ApplySubGear(startingGear, profile, prototype);

            EquipStartingGear(entity.Value, startingGear, raiseEvent: false);
            _internalEncryption.TryInsertEncryptionKey(entity.Value, startingGear, EntityManager);
        }

        var gearEquippedEv = new StartingGearEquippedEvent(entity.Value);
        RaiseLocalEvent(entity.Value, ref gearEquippedEv);

        if (profile != null)
        {
            if (prototype != null)
                SetPdaAndIdCardData(entity.Value, profile.Name, prototype, station);

            _humanoidSystem.LoadProfile(entity.Value, profile, loadExtensions: false, generateLoadouts: false);
            _metaSystem.SetEntityName(entity.Value, profile.Name);
            if (profile.FlavorText != "" && _configurationManager.GetCVar(CCVars.FlavorText))
                EnsureComp<DetailExaminableComponent>(entity.Value).Content = profile.FlavorText;
        }

        DoJobSpecials(job, entity.Value);
        _identity.QueueIdentityUpdate(entity.Value);
        return entity.Value;
    }

    private void DoJobSpecials(ProtoId<JobPrototype>? job, EntityUid entity)
    {
        if (!_prototypeManager.TryIndex(job ?? string.Empty, out JobPrototype? prototype))
            return;

        foreach (var jobSpecial in prototype.Special)
            jobSpecial.AfterEquip(entity);
    }

    /// Sets the ID card and PDA name, job, and access data.
    /// </summary>
    /// <param name="entity">Entity to load out.</param>
    /// <param name="characterName">Character name to use for the ID.</param>
    /// <param name="jobPrototype">Job prototype to use for the PDA and ID.</param>
    /// <param name="station">The station this player is being spawned on.</param>
    public void SetPdaAndIdCardData(EntityUid entity, string characterName, JobPrototype jobPrototype, EntityUid? station)
    {
        if (!InventorySystem.TryGetSlotEntity(entity, "id", out var idUid))
            return;

        var cardId = idUid.Value;
        if (TryComp<PdaComponent>(idUid, out var pdaComponent) && pdaComponent.ContainedId != null)
            cardId = pdaComponent.ContainedId.Value;

        if (!TryComp<IdCardComponent>(cardId, out var card))
            return;

        _cardSystem.TryChangeFullName(cardId, characterName, card);
        _cardSystem.TryChangeJobTitle(cardId, jobPrototype.LocalizedName, card);

        if (_prototypeManager.TryIndex(jobPrototype.Icon, out var jobIcon))
            _cardSystem.TryChangeJobIcon(cardId, jobIcon, card);

        var extendedAccess = false;
        if (station != null)
        {
            var data = Comp<StationJobsComponent>(station.Value);
            extendedAccess = data.ExtendedAccess;
        }

        _accessSystem.SetAccessToJob(cardId, jobPrototype, extendedAccess);

        if (pdaComponent != null)
            _pdaSystem.SetOwner(idUid.Value, pdaComponent, entity, characterName);
        //IH - Start
        var preset = EnsureComp<PresetIdCardComponent>(cardId);
        preset.JobName = jobPrototype.ID;

        if (TryComp<EconomyAccountHolderComponent>(cardId, out var economyAccountHolder))
        {
            economyAccountHolder.AccountName = characterName;
            Dirty(cardId, economyAccountHolder);

            if (ShouldAssignBankAccount(entity, station))
                _bankAccountSystem.TryActivate((cardId, economyAccountHolder), out _);
        }
    }

    private bool ShouldAssignBankAccount(EntityUid entity, EntityUid? station, TransformComponent? xform = null)
    {
        if (!Resolve(entity, ref xform, logMissing: false))
            return false;

        if (station != null && IsEligibleMainStation(station.Value))
            return true;

        if (xform.GridUid is EntityUid grid && HasComp<LavalandStationComponent>(grid))
            return true;

        var defaultMap = _gameTicker.DefaultMap;
        if (defaultMap == MapId.Nullspace)
            return false;

        var mainStation = _stationSystem.GetStationInMap(defaultMap);
        if (mainStation == null)
            return false;

        var owningStation = _stationSystem.GetOwningStation(entity, xform);
        return owningStation == mainStation;
    }

    private bool IsEligibleMainStation(EntityUid station)
    {
        var defaultMap = _gameTicker.DefaultMap;
        if (defaultMap == MapId.Nullspace)
            return false;

        var mainStation = _stationSystem.GetStationInMap(defaultMap);
        return mainStation == station;
        //IH - end
    }


    #endregion Player spawning helpers
}

/// <summary>
/// Ordered broadcast event fired on any spawner eligible to attempt to spawn a player.
/// This event's success is measured by if SpawnResult is not null.
/// You should not make this event's success rely on random chance.
/// This event is designed to use ordered handling. You probably want SpawnPointSystem to be the last handler.
/// </summary>
[PublicAPI]
public sealed class PlayerSpawningEvent : EntityEventArgs
{
    /// <summary>
    /// The entity spawned, if any. You should set this if you succeed at spawning the character, and leave it alone if it's not null.
    /// </summary>
    public EntityUid? SpawnResult;
    /// <summary>
    /// The job to use, if any.
    /// </summary>
    public readonly ProtoId<JobPrototype>? Job;
    /// <summary>
    /// The profile to use, if any.
    /// </summary>
    public readonly HumanoidCharacterProfile? HumanoidCharacterProfile;
    /// <summary>
    /// The target station, if any.
    /// </summary>
    public readonly EntityUid? Station;
    /// <summary>
    /// Desired SpawnPointType, if any.
    /// </summary>
    public readonly SpawnPointType DesiredSpawnPointType;

    public PlayerSpawningEvent(ProtoId<JobPrototype>? job, HumanoidCharacterProfile? humanoidCharacterProfile, EntityUid? station, SpawnPointType spawnPointType = SpawnPointType.Unset)
    {
        Job = job;
        HumanoidCharacterProfile = humanoidCharacterProfile;
        Station = station;
        DesiredSpawnPointType = spawnPointType;
    }
}
