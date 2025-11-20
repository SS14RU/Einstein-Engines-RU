using System;
using System.Numerics;
using Content.Server.Gravity;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Gravity;
using Robust.Shared.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Server.AWS.Gravity;

/// <summary>
///     AWS-specific overrides for gravity generator grid interactions (mass multipliers, FTL blocking, etc.).
/// </summary>
public sealed class AwsGravityGeneratorFieldSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private EntityQuery<MapGridComponent> _gridQuery = default!;
    private EntityQuery<PhysicsComponent> _physicsQuery = default!;
    private EntityQuery<GridGravityWellComponent> _gravityWellQuery = default!;
    private EntityQuery<FixturesComponent> _fixturesQuery = default!;

    public override void Initialize()
    {
        base.Initialize();
        _gridQuery = GetEntityQuery<MapGridComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _gravityWellQuery = GetEntityQuery<GridGravityWellComponent>();
        _fixturesQuery = GetEntityQuery<FixturesComponent>();

        SubscribeLocalEvent<GravityGeneratorComponent, AwsGravityGeneratorParentChangedEvent>(OnParentChanged);
        SubscribeLocalEvent<GravityGeneratorComponent, AwsGravityGeneratorActivatedEvent>(OnActivated);
        SubscribeLocalEvent<GravityGeneratorComponent, AwsGravityGeneratorDeactivatedEvent>(OnDeactivated);
        SubscribeLocalEvent<GravityGeneratorComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GravityGeneratorComponent, PowerChargeComponent>();
        while (query.MoveNext(out var uid, out var grav, out var charge))
        {
            var switchedOn = charge.SwitchedOn;

            if (!switchedOn && grav.CurrentGrid is { } offGrid && !grav.GravityActive)
            {
                DisableField(uid, grav);
            }
            else if (switchedOn && grav.CurrentGrid is null)
            {
                TryEnableField(uid, grav);
            }

            if (grav.CurrentGrid is not { } grid || !_gravityWellQuery.TryGetComponent(grid, out var well))
                continue;

            var effectiveMultiplier = GetEffectiveMassMultiplier(uid, grav, charge);
            if (well.TryUpdateGeneratorMultiplier(uid, effectiveMultiplier))
                ApplyGridEffects(grid, well);
        }
    }

    private void OnActivated(Entity<GravityGeneratorComponent> ent, ref AwsGravityGeneratorActivatedEvent args)
    {
        TryEnableField(ent.Owner, ent.Comp);
    }

    private void OnDeactivated(Entity<GravityGeneratorComponent> ent, ref AwsGravityGeneratorDeactivatedEvent args)
    {
        DisableField(ent.Owner, ent.Comp);
    }

    private void OnShutdown(Entity<GravityGeneratorComponent> ent, ref ComponentShutdown args)
    {
        DisableField(ent.Owner, ent.Comp);
    }

    private void OnParentChanged(EntityUid uid, GravityGeneratorComponent component, ref AwsGravityGeneratorParentChangedEvent args)
    {
        if (!component.GravityActive)
            return;

        if (args.OldParent != null && args.OldParent.Value == component.CurrentGrid)
            RemoveFromGrid(args.OldParent.Value, uid, component);

        TryEnableField(uid, component);
    }

    private void TryEnableField(EntityUid generator, GravityGeneratorComponent component)
    {
        if (!TryGetParentGrid(generator, out var gridUid))
            return;

        var well = EnsureComp<GridGravityWellComponent>(gridUid);
        var effectiveMultiplier = GetEffectiveMassMultiplier(generator, component);
        var blocksFtl = component.GravityActive && component.BlocksFtl;
        well.SetGenerator(generator, effectiveMultiplier, component.ProtectRadius, blocksFtl);
        ApplyGridEffects(gridUid, well);
        component.CurrentGrid = gridUid;

        if (component.GravityActive && _physicsQuery.TryGetComponent(gridUid, out var body))
        {
            _physics.SetLinearVelocity(gridUid, Vector2.Zero, body: body);
            _physics.SetAngularVelocity(gridUid, 0f, body: body);
        }
    }

    private void DisableField(EntityUid generator, GravityGeneratorComponent component)
    {
        if (component.CurrentGrid is not { } grid)
            return;

        RemoveFromGrid(grid, generator, component);
    }

    private void RemoveFromGrid(EntityUid gridUid, EntityUid generator, GravityGeneratorComponent component)
    {
        if (!_gravityWellQuery.TryGetComponent(gridUid, out var well))
        {
            component.CurrentGrid = null;
            return;
        }

        if (!well.RemoveGenerator(generator))
            return;

        component.CurrentGrid = null;

        ApplyGridEffects(gridUid, well);
        if (!well.Active)
            RemCompDeferred<GridGravityWellComponent>(gridUid);
    }

    private bool TryGetParentGrid(EntityUid uid, out EntityUid gridUid)
    {
        gridUid = EntityUid.Invalid;

        if (!TryComp(uid, out TransformComponent? xform))
            return false;

        var parent = xform.ParentUid;

        if (!parent.IsValid())
            return false;

        if (!_gridQuery.HasComponent(parent))
            return false;

        gridUid = parent;
        return true;
    }

    private void ApplyGridEffects(EntityUid gridUid, GridGravityWellComponent well)
    {
        UpdateGridMass(gridUid, well);
    }

    private void UpdateGridMass(EntityUid gridUid, GridGravityWellComponent well)
    {
        if (!_fixturesQuery.TryGetComponent(gridUid, out var fixtures))
        {
            well.ClearBaseDensities();
            return;
        }

        var targetMultiplier = MathF.Max(1f, well.MassMultiplier);

        if (MathHelper.CloseTo(targetMultiplier, 1f))
        {
            if (!MathHelper.CloseTo(well.AppliedMassMultiplier, 1f))
            {
                foreach (var (id, fixture) in fixtures.Fixtures)
                {
                    if (!well.TryGetBaseDensity(id, out var baseDensity))
                        continue;

                    _physics.SetDensity(gridUid, id, fixture, baseDensity, manager: fixtures);
                }
            }

            well.ClearBaseDensities();
            return;
        }

        foreach (var (id, fixture) in fixtures.Fixtures)
        {
            if (!well.TryGetBaseDensity(id, out var baseDensity))
            {
                baseDensity = fixture.Density;
                well.RememberBaseDensity(id, baseDensity);
            }

            _physics.SetDensity(gridUid, id, fixture, baseDensity * targetMultiplier, manager: fixtures);
        }

        well.AppliedMassMultiplier = targetMultiplier;
    }

    private float GetEffectiveMassMultiplier(EntityUid generator, GravityGeneratorComponent component, PowerChargeComponent? chargeComp = null)
    {
        var maxMultiplier = MathF.Max(component.MassMultiplier, 1f);
        var chargeLevel = chargeComp?.Charge ??
            (TryComp(generator, out PowerChargeComponent? storedCharge) ? storedCharge.Charge : 1f);

        chargeLevel = Math.Clamp(chargeLevel, 0f, 1f);
        return MathHelper.Lerp(1f, maxMultiplier, chargeLevel);
    }

    [ByRefEvent]
    public readonly record struct AwsGravityGeneratorActivatedEvent(Entity<GravityGeneratorComponent> Generator);

    [ByRefEvent]
    public readonly record struct AwsGravityGeneratorDeactivatedEvent(Entity<GravityGeneratorComponent> Generator);

    [ByRefEvent]
    public readonly record struct AwsGravityGeneratorParentChangedEvent(Entity<GravityGeneratorComponent> Generator, EntityUid? OldParent);
}
