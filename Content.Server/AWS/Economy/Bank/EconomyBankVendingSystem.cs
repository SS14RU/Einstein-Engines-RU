using System;
using System.Collections.Generic;
using Content.Server.Cargo.Systems;
using Content.Server.Power.Components;
using Content.Server.VendingMachines;
using Content.Shared.AWS.Economy.Bank;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Emag.Components;
using Content.Shared.Storage.Components;
using Content.Shared.VendingMachines;
using Robust.Shared.Localization;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.AWS.Economy.Bank;

public sealed class EconomyBankVendingSystem : EntitySystem
{
    [Dependency] private readonly EconomyBankAccountSystem _bankAccountSystem = default!;
    [Dependency] private readonly PricingSystem _pricing = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly VendingMachineSystem _vendingMachineSystem = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    private const double StationMarginRate = 0.2;
    private readonly Dictionary<string, double> _inventoryWholesaleCost = new();
    private bool _wholesaleCacheBuilt;

    public override void Initialize()
    {
        SubscribeLocalEvent<VendingMachineComponent, VendingMachineSelectAttemptEvent>(OnVendingSelect);
        SubscribeLocalEvent<VendingMachineComponent, VendingMachineRecalculatePriceEvent>(OnRecalculatePrice);
    }

    private void OnVendingSelect(EntityUid uid, VendingMachineComponent component, VendingMachineSelectAttemptEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<EconomyBankTerminalComponent>(uid, out var terminal))
            return;

        if (HasComp<EmaggedComponent>(uid))
            return;

        if (args.Entry is not { } entry || entry.Price == 0)
            return;

        var itemName = args.ID;
        if (_prototypeManager.TryIndex<EntityPrototype>(args.ID, out var productProto))
            itemName = productProto.Name;

        _bankAccountSystem.UpdateTerminal((uid, terminal),
            entry.Price,
            Loc.GetString("economyBankTerminal-component-vending-reason", ("itemName", itemName)));

        component.SelectedItemInventoryType = args.Type;
        component.SelectedItemId = args.ID;
        args.Handled = true;
    }

    private void OnRecalculatePrice(EntityUid uid, VendingMachineComponent component, ref VendingMachineRecalculatePriceEvent args)
    {
        if (args.Handled)
            return;

        EnsureWholesaleMap();

        double baseTotal = 0;

        foreach (var entry in component.Inventory.Values)
        {
            if (!_prototypeManager.TryIndex<EntityPrototype>(entry.ID, out var prototype))
                continue;

            var estimate = GetPrototypeEstimate(prototype, new HashSet<string>());
            if (estimate <= 0)
                continue;

            baseTotal += estimate * entry.Amount;
        }

        if (baseTotal <= 0)
            return;

        var factor = 1 + StationMarginRate;

        if (_inventoryWholesaleCost.TryGetValue(component.PackPrototypeId, out var wholesaleCost) && wholesaleCost > 0)
            factor = (wholesaleCost * (1 + StationMarginRate)) / baseTotal;

        var dirty = false;

        foreach (var entry in _vendingMachineSystem.GetAllInventory(uid, component))
        {
            if (entry == null)
                continue;

            if (!_prototypeManager.TryIndex<EntityPrototype>(entry.ID, out var prototype))
                continue;

            var estimate = GetPrototypeEstimate(prototype, new HashSet<string>());
            if (estimate <= 0)
                continue;

            var price = (ulong) Math.Max(1, Math.Ceiling(estimate * factor));
            if (entry.Price == price)
                continue;

            entry.Price = price;
            dirty = true;
        }

        if (dirty)
            Dirty(uid, component);

        args.Handled = true;
    }

    private double GetPrototypeEstimate(EntityPrototype prototype, HashSet<string> visited)
    {
        if (!visited.Add(prototype.ID))
            return 0;

        var price = _pricing.GetEstimatedPrice(prototype);
        price += GetBatteryPrice(prototype);
        price += GetItemSlotStartingItemPrice(prototype, visited);
        visited.Remove(prototype.ID);
        return price;
    }

    private double GetBatteryPrice(EntityPrototype prototype)
    {
        if (!prototype.Components.TryGetValue(_componentFactory.GetComponentName(typeof(BatteryComponent)), out var batteryEntry))
            return 0;

        if (batteryEntry.Component is not BatteryComponent battery)
            return 0;

        return battery.CurrentCharge * battery.PricePerJoule;
    }

    private double GetItemSlotStartingItemPrice(EntityPrototype prototype, HashSet<string> visited)
    {
        var componentName = _componentFactory.GetComponentName(typeof(ItemSlotsComponent));
        if (!prototype.Components.TryGetValue(componentName, out var componentEntry))
            return 0;

        if (componentEntry.Component is not ItemSlotsComponent itemSlots)
            return 0;

        var price = 0.0;

        foreach (var slot in itemSlots.Slots.Values)
        {
            if (slot.StartingItem is not { } startingItem)
                continue;

            if (!_prototypeManager.TryIndex<EntityPrototype>(startingItem, out var itemPrototype))
                continue;

            price += GetPrototypeEstimate(itemPrototype, visited);
        }

        return price;
    }

    private void EnsureWholesaleMap()
    {
        if (_wholesaleCacheBuilt)
            return;

        _wholesaleCacheBuilt = true;

        foreach (var cargoProto in _prototypeManager.EnumeratePrototypes<CargoProductPrototype>())
        {
            if (!cargoProto.ID.StartsWith("CrateVendingMachineRestock", StringComparison.Ordinal))
                continue;

            if (!_prototypeManager.TryIndex<EntityPrototype>(cargoProto.Product, out var crateProto))
                continue;

            var restockInfos = new List<IEnumerable<string>>();
            var totalBoxes = 0;

            foreach (var componentEntry in crateProto.Components.Values)
            {
                if (componentEntry.Component is not StorageFillComponent storageFill)
                    continue;

                foreach (var spawnEntry in storageFill.Contents)
                {
                    if (spawnEntry.PrototypeId is not { } restockProtoId)
                        continue;

                    if (!_prototypeManager.TryIndex<EntityPrototype>(restockProtoId, out var restockProto))
                        continue;

                    if (!restockProto.Components.TryGetValue(nameof(VendingMachineRestockComponent), out var restockCompEntry))
                        continue;

                    if (restockCompEntry.Component is not VendingMachineRestockComponent restockComponent)
                        continue;

                    var amount = Math.Max(spawnEntry.Amount, 1);
                    totalBoxes += amount;
                    restockInfos.Add(restockComponent.CanRestock);
                }
            }

            if (totalBoxes == 0)
                continue;

            var costPerBox = cargoProto.Cost / (double) totalBoxes;

            foreach (var inventories in restockInfos)
            {
                foreach (var inventory in inventories)
                {
                    if (_inventoryWholesaleCost.TryGetValue(inventory, out var existing) && existing <= costPerBox)
                        continue;

                    _inventoryWholesaleCost[inventory] = costPerBox;
                }
            }
        }
    }
}
