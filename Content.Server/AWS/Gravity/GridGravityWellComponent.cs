//IH - Start
using System;
using System.Collections.Generic;
using Robust.Shared.Maths;

namespace Content.Server.Gravity;

/// <summary>
///     Tracks active gravity generator fields on a grid so other systems can query thrust modifiers or FTL locks.
/// </summary>
[RegisterComponent]
public sealed partial class GridGravityWellComponent : Component
{
    private readonly Dictionary<EntityUid, GridGravityWellGeneratorInfo> _generators = new();
    private readonly Dictionary<string, float> _baseFixtureDensities = new();

    [ViewVariables] public float MassMultiplier { get; private set; } = 1f;
    // TODO: ProtectRadius понадобится, когда эффекты гравигена ограничим зонами; сейчас храним значение в преддверии следующих шагов.
    [ViewVariables] public float ProtectRadius { get; private set; }
    [ViewVariables] public bool BlocksFtl { get; private set; }
    [ViewVariables] public float AppliedMassMultiplier { get; set; } = 1f;

    public bool Active => _generators.Count > 0;

    /// <summary>
    ///     Value applied to shuttle thrust / torque to simulate increased mass.
    /// </summary>
    public float ThrustScale => MassMultiplier <= 1f ? 1f : 1f / MassMultiplier;

    public void SetGenerator(EntityUid uid, float massMultiplier, float radius, bool blockFtl)
    {
        _generators[uid] = new GridGravityWellGeneratorInfo(massMultiplier, radius, blockFtl);
        Recalculate();
    }

    public bool RemoveGenerator(EntityUid uid)
    {
        if (!_generators.Remove(uid))
            return false;

        Recalculate();
        return true;
    }

    private void Recalculate()
    {
        if (_generators.Count == 0)
        {
            MassMultiplier = 1f;
            ProtectRadius = 0f;
            BlocksFtl = false;
            return;
        }

        var maxMultiplier = 1f;
        var maxRadius = 0f;
        var blockFtl = false;

        foreach (var entry in _generators.Values)
        {
            maxMultiplier = MathF.Max(maxMultiplier, entry.MassMultiplier);
            maxRadius = MathF.Max(maxRadius, entry.Radius);
            blockFtl |= entry.BlocksFtl;
        }

        MassMultiplier = maxMultiplier;
        ProtectRadius = maxRadius;
        BlocksFtl = blockFtl;
    }

    public bool TryGetBaseDensity(string id, out float density)
        => _baseFixtureDensities.TryGetValue(id, out density);

    public void RememberBaseDensity(string id, float density)
        => _baseFixtureDensities[id] = density;

    public void ClearBaseDensities()
    {
        _baseFixtureDensities.Clear();
        AppliedMassMultiplier = 1f;
    }

    public bool TryUpdateGeneratorMultiplier(EntityUid uid, float newMultiplier)
    {
        if (!_generators.TryGetValue(uid, out var existing))
            return false;

        if (MathHelper.CloseTo(existing.MassMultiplier, newMultiplier))
            return false;

        _generators[uid] = existing with { MassMultiplier = newMultiplier };
        Recalculate();
        return true;
    }
}

public readonly record struct GridGravityWellGeneratorInfo(float MassMultiplier, float Radius, bool BlocksFtl);
//IH - End
