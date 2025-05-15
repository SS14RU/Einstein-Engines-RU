using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Forensics;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.AWS.Economy.Bank;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Preferences;
using Content.Shared.AWS.Economy.Insurance;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.AWS.Economy.Insurance;

public sealed class EconomyInsuranceSystem : EconomyInsuranceSystemShared
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawn, after: new[] { typeof(SpawnPointSystem) });
        SubscribeLocalEvent<EconomyInsuranceServerComponent, ComponentAdd>(OnComponentAdd);
    }

    private void OnComponentAdd(EntityUid uid, EconomyInsuranceServerComponent component, ComponentAdd args)
    {
        if (TryGetServer(out var _))
        {
            RemComp(uid, component);
            DebugTools.Assert("Only one supported server can be exists at once in the world");
        }
    }

    private void OnPlayerSpawn(PlayerSpawningEvent ev)
    {
        if (ev.SpawnResult is null || ev.HumanoidCharacterProfile is null)
            DebugTools.Assert("Unable to proccess insurance on player spawn!");

        var playerUid = ev.SpawnResult.Value;
        var profile = ev.HumanoidCharacterProfile;

        var preparedData = PerformPrepareData(playerUid, profile);

        if (preparedData is null)
            DebugTools.Assert($"Unable to proccess insurance by getting necessary components");

        //if (!TryCreateInsuranceRecord(preparedData.InsurancePrototype,
        //        preparedData.InsurerName,
        //        preparedData.InsurerAccountId,
        //        preparedData.InsurerDna,
        //        out var economyInsuranceInfo,
        //        out var error))
        //    DebugTools.Assert($"Unable to create insurance record for {playerUid}!\n{error}");
    }

    private PreparedInsurerData? PerformPrepareData(EntityUid playerUid, HumanoidCharacterProfile profile)
    {
        if (!TryComp<DnaComponent>(playerUid, out var dnaComponent))
            return null;

        if (!_inventorySystem.TryGetSlotEntity(playerUid, "id", out var pdaUid))
            return null;

        if (!TryComp<PdaComponent>(pdaUid, out var pdaComponent) || pdaComponent.ContainedId is null)
            return null;

        var cardUid = pdaComponent.ContainedId;

        if (!TryComp<IdCardComponent>(cardUid, out var cardComponent) || cardComponent.FullName is null)
            return null;

        if (!TryComp<EconomyAccountHolderComponent>(cardUid, out var accountHolderComponent))
            return null;

        return new(profile.Insurance, cardComponent.FullName, accountHolderComponent.AccountID, dnaComponent.DNA);
    }

    [PublicAPI]
    public bool TryCreateInsuranceRecord(ProtoId<EconomyInsurancePrototype> insuranceProto,
        string insurerName,
        string payerAccountId,
        string insurerDna,
        [NotNullWhen(true)] out EconomyInsuranceInfo? economyInsuranceInfo,
        [NotNullWhen(false)] out string? error)
    {
        error = null;
        economyInsuranceInfo = null;

        if (!TryGetServer(out var server))
        {
            error = "Not found server";
            return false;
        }

        var comp = server.Value.Comp;

        if (comp.InsuranceInfo.Any(x => x.DNA == insurerDna || x.InsurerName == insurerName))
        {
            error = "Already exists record with provided data";
            return false;
        }

        var insuranceRecord = CreateInsuranceRecord(server, insuranceProto, insurerName, payerAccountId, insurerDna);

        economyInsuranceInfo = insuranceRecord;

        return true;
    }

    private EconomyInsuranceInfo CreateInsuranceRecord(EconomyInsuranceServerComponent serverComponent,
        ProtoId<EconomyInsurancePrototype> insuranceProto,
        string insurerName,
        string payerAccountId,
        string insurerDna)
    {
        EconomyInsuranceInfo economyInsuranceInfo = new(insuranceProto, insurerName, payerAccountId, insurerDna);
        serverComponent.InsuranceInfo.Add(economyInsuranceInfo);

        return economyInsuranceInfo;
    }

    private record PreparedInsurerData(
        ProtoId<EconomyInsurancePrototype> InsurancePrototype,
        string InsurerName,
        string InsurerAccountId,
        string InsurerDna);
}
