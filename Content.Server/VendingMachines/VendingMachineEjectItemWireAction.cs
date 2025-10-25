using Content.Server.Wires;
using Content.Shared.VendingMachines;
using Content.Shared.Wires;

namespace Content.Server.VendingMachines;

public sealed partial class VendingMachineEjectItemWireAction : ComponentWireAction<VendingMachineComponent>
{
    private VendingMachineSystem _vendingMachineSystem = default!;

    public override Color Color { get; set; } = Color.Red;
    public override string Name { get; set; } = "wire-name-vending-eject";

    public override object? StatusKey { get; } = EjectWireKey.StatusKey;

    //SS14RU - start
    public override StatusLightState? GetLightState(Wire wire, VendingMachineComponent comp)
        => comp.Disabled
            ? StatusLightState.Off
            : comp.CanShoot
                ? StatusLightState.BlinkingFast
                : StatusLightState.On;
    //SS14RU - end

    public override void Initialize()
    {
        base.Initialize();

        _vendingMachineSystem = EntityManager.System<VendingMachineSystem>();
    }

    public override bool Cut(EntityUid user, Wire wire, VendingMachineComponent vending)
    {
        //SS14RU - start
        _vendingMachineSystem.SetDisabled(wire.Owner, true, vending);
        //SS14RU - end
        _vendingMachineSystem.SetShooting(wire.Owner, true, vending);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, VendingMachineComponent vending)
    {
        _vendingMachineSystem.SetShooting(wire.Owner, false, vending);
        //SS14RU - start
        _vendingMachineSystem.SetDisabled(wire.Owner, false, vending);
        //SS14RU - end
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, VendingMachineComponent vending)
    {
        _vendingMachineSystem.TryWireEject(wire.Owner, vending); //SS14RU - edit
    }
}
