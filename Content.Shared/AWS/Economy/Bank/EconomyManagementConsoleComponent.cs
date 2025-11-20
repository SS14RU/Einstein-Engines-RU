using Content.Shared.Containers.ItemSlots;
using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using System.Collections.Generic;

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

    [DataField("defaultJob")]
    public ProtoId<JobPrototype> DefaultJob = "Passenger";

    [DataField("salaryButtonSteps")]
    public List<int> SalaryButtonSteps { get; set; } = new() { -10, -1, 1, 10 };

    [DataField("allowCentralCommandAccount")]
    public bool AllowCentralCommandAccount = false;

    [DataField("allowRestrictedAccounts")]
    public bool AllowRestrictedAccounts = false;

    [DataField("allowedAccountMask")]
    public EconomyBankAccountMask AllowedAccountMask = EconomyBankAccountMask.All;

    [DataField("allowedAccountTags")]
    public List<BankAccountTag>? AllowedAccountTags;
}

[Serializable, NetSerializable]
public enum EconomyManagementConsoleUiKey
{
    Key
}
