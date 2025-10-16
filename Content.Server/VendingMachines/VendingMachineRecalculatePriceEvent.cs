using Content.Shared.VendingMachines;

namespace Content.Server.VendingMachines;

public sealed class VendingMachineRecalculatePriceEvent : HandledEntityEventArgs
{
    public EntityUid VendingMachine { get; }
    public VendingMachineComponent Component { get; }

    public VendingMachineRecalculatePriceEvent(EntityUid vendingMachine, VendingMachineComponent component)
    {
        VendingMachine = vendingMachine;
        Component = component;
    }
}
