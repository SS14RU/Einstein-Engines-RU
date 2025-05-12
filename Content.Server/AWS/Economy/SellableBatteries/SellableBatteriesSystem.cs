using Content.Server.Cargo.Systems;
using Content.Server.Power.Components;
using Content.Shared.AWS.Economy.SellableBatteries;
using Content.Server.NodeContainer;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Robust.Shared.Utility;

namespace Content.Server.AWS.Economy.SellableBatteries;

public sealed class SellableBatteriesSystem : EntitySystem
{

    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SellableBatteryComponent, PriceCalculationEvent>(OnSellableBatteryPrice);
        SubscribeLocalEvent<SellableBatteryComponent, AnchorStateChangedEvent>(OnSellableBatteryAnchorState);
    }

    private void OnSellableBatteryPrice(EntityUid ent, SellableBatteryComponent comp, ref PriceCalculationEvent args)
    {
        if (!TryComp<BatteryComponent>(ent, out var batteryComponent))
            return;

        args.Price += float.Ceiling(CalculateAdditionalBatteryCost(batteryComponent, comp));
    }

    private float CalculateAdditionalBatteryCost(BatteryComponent batteryComponent, SellableBatteryComponent sellableBatteryComponent)
    {
        return batteryComponent.MaxCharge / batteryComponent.CurrentCharge * sellableBatteryComponent.PricePerChargedPrecent;
    }

    private void OnSellableBatteryAnchorState(EntityUid batteryUid, SellableBatteryComponent comp, ref AnchorStateChangedEvent args)
    {
        var batteryTransformComp = Transform(batteryUid);
        if (batteryTransformComp.GridUid is not { } gridUid)
            return;

        if (!TryFindCharger(gridUid, batteryTransformComp.LocalPosition.Ceiled(), out var charger))
            return;

        if (args.Anchored)
            if (TryComp<BatteryComponent>(batteryUid, out var batteryComp)
                && TryComp<NodeContainerComponent>(batteryUid, out var nodeContainerComp))
            {
                AttachBattery((batteryUid, comp, batteryComp, nodeContainerComp), charger.Value);

                return;
            }

        DeAttachBattery(charger.Value);
    }

    public bool TryFindCharger(EntityUid gridUid, Vector2i pos, [NotNullWhen(true)] out Entity<SellableBatteryProxySwitcherComponent>? charger)
    {
        charger = null;

        HashSet<Entity<SellableBatteryProxySwitcherComponent>> chargers = new();
        _lookupSystem.GetLocalEntitiesIntersecting(gridUid, pos, chargers);

        if (chargers.Count > 1)
        {
            DebugTools.Assert($"Error configured prototype when applied {typeof(SellableBatteryProxySwitcherComponent)}! Only 1 charger can be at 1 tile!");
            return false;
        }

        charger = chargers.Single();
        return true;
    }

    private void AttachBattery(Entity<SellableBatteryComponent, BatteryComponent, NodeContainerComponent> battery, Entity<SellableBatteryProxySwitcherComponent> charger)
    {
        charger.Comp.Connected = true;
        charger.Comp.ConnectedBattery = battery;

        Dirty(charger);
    }

    private void DeAttachBattery(Entity<SellableBatteryProxySwitcherComponent> charger)
    {
        charger.Comp.Connected = false;

        Dirty(charger);
    }
}
