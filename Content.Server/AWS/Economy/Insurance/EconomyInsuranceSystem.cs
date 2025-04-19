using System.Diagnostics.CodeAnalysis;
using Content.Server.Station.Systems;
using JetBrains.Annotations;

namespace Content.Shared.AWS.Economy.Insurance;

public sealed class EconomyInsuranceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawn);
        SubscribeLocalEvent<EconomyInsuranceServerComponent, ComponentAdd>(OnComponentAdd);
    }

    private void OnComponentAdd(EntityUid uid, EconomyInsuranceServerComponent component, ComponentAdd args)
    {
        if (TryGetServer(out var _))
            throw new Exception("Only one supported server can be exists at once in the world");
    }

    private void OnPlayerSpawn(PlayerSpawningEvent ev)
    {
    }

    [PublicAPI]
    public bool TryGetServer([NotNullWhen(true)] out Entity<EconomyInsuranceServerComponent>? server)
    {
        server = GetServer();

        if (server is not null)
            return true;

        return false;
    }

    private Entity<EconomyInsuranceServerComponent>? GetServer()
    {
        return null;
    }
}