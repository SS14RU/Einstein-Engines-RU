using System;
using Content.Server.Gravity;
using Content.Shared.AWS.Gravity;
using Content.Shared.Examine;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;

namespace Content.Server.AWS.Gravity;

/// <summary>
///     Updates networked gravity generator status data (station mass / FTL lock) without touching the base power systems.
/// </summary>
public sealed class AwsGravityGeneratorStatusSystem : EntitySystem
{
    private EntityQuery<PhysicsComponent> _physicsQuery = default!;
    private EntityQuery<GridGravityWellComponent> _gravityWellQuery = default!;

    public override void Initialize()
    {
        base.Initialize();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _gravityWellQuery = GetEntityQuery<GridGravityWellComponent>();
        SubscribeLocalEvent<AwsGravityGeneratorStatusComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AwsGravityGeneratorStatusComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var status, out var xform))
        {
            UpdateStatus(uid, status, xform);
        }
    }

    private void UpdateStatus(EntityUid uid, AwsGravityGeneratorStatusComponent status, TransformComponent xform)
    {
        var showStatus = TryGetStationData(xform, out var mass, out var locked);

        if (status.ShowStatus == showStatus)
        {
            if (!showStatus || (MathHelper.CloseTo(status.StationMass, mass) && status.StationFtlLocked == locked))
                return;
        }

        status.ShowStatus = showStatus;
        if (showStatus)
        {
            status.StationMass = mass;
            status.StationFtlLocked = locked;
        }
        else
        {
            status.StationMass = 0f;
            status.StationFtlLocked = false;
        }

        Dirty(uid, status);
    }

    private bool TryGetStationData(TransformComponent xform, out float mass, out bool locked)
    {
        mass = 0f;
        locked = false;

        if (xform.GridUid is not { } gridUid)
            return false;

        if (!_physicsQuery.TryGetComponent(gridUid, out var physics))
            return false;

        mass = physics.Mass;
        locked = _gravityWellQuery.TryGetComponent(gridUid, out var well) && well.BlocksFtl;
        return true;
    }

    private void OnExamined(Entity<AwsGravityGeneratorStatusComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var status = ent.Comp;
        if (!status.ShowStatus)
            return;

        args.PushMarkup(Loc.GetString("gravity-generator-examine-mass", ("mass", Math.Round(status.StationMass, 1))));

        var stateKey = status.StationFtlLocked ? "gravity-generator-examine-ftl-locked" : "gravity-generator-examine-ftl-free";
        args.PushMarkup(Loc.GetString("gravity-generator-examine-ftl", ("state", Loc.GetString(stateKey))));
    }
}
