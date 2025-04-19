using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy.Bank;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EconomyManagementConsoleComponent : Component
{
    public const string ConsoleCardID = "ManagementConsole-IdSlot";
    public const string TargetCardID = "ManagementConsole-Target-IdSlot";

    [DataField("cardSlot"), AutoNetworkedField]
    public ItemSlot CardSlot = new();

    [DataField("targetCardSlot"), AutoNetworkedField]
    public ItemSlot TargetCardSlot = new();
}

[Serializable, NetSerializable]
public enum EconomyManagementConsoleUiKey
{
    Key
}
