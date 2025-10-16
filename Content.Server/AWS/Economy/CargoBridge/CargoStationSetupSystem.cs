using Content.Server.Cargo.Systems;
using Content.Server.Station.Systems;

using Robust.Shared.GameObjects;

namespace Content.Server.AWS.Economy.CargoBridge;

public sealed class CargoStationSetupSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialized);
    }

    private void OnStationInitialized(StationInitializedEvent args)
    {
        if (HasComp<EconomyThalerCargoComponent>(args.Station))
            return;

        var component = EnsureComp<EconomyThalerCargoComponent>(args.Station);
        if (string.IsNullOrWhiteSpace(component.AccountId))
            component.AccountId = "NT-Cargo";
    }
}
