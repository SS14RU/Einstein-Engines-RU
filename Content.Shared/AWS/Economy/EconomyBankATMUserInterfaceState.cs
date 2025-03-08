using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy;

[Serializable, NetSerializable]
public sealed class EconomyBankATMUserInterfaceState : BoundUserInterfaceState
{
    public EconomyBankATMAccountInfo? BankAccount;
    public string? Error;
}

[Serializable, NetSerializable]
public sealed class EconomyBankATMAccountInfo
{
    public ulong Balance;
    public string AccountId = "";
    public string AccountName = "";
    public bool Blocked;
}

[Serializable, NetSerializable]
public sealed class EconomyBankATMWithdrawMessage(ulong amount) : BoundUserInterfaceMessage
{
    public readonly ulong Amount = amount;
}

[Serializable, NetSerializable]
public sealed class EconomyBankATMTransferMessage(ulong amount, string recipientAccountId) : BoundUserInterfaceMessage
{
    public readonly ulong Amount = amount;
    public readonly string RecipientAccountId = recipientAccountId;
}

[Serializable, NetSerializable]
public enum EconomyBankATMUiKey
{
    Key
}
