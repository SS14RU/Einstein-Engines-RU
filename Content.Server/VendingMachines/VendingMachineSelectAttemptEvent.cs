using Content.Shared.VendingMachines;

namespace Content.Server.VendingMachines;

public sealed class VendingMachineSelectAttemptEvent : HandledEntityEventArgs
{
    public EntityUid? Actor { get; }
    public InventoryType Type { get; }
    public string ID { get; }
    public VendingMachineInventoryEntry? Entry { get; }

    public VendingMachineSelectAttemptEvent(EntityUid? actor, InventoryType type, string id, VendingMachineInventoryEntry? entry)
    {
        Actor = actor;
        Type = type;
        ID = id;
        Entry = entry;
    }
}
