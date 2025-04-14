using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy.Bank;

[Serializable, NetSerializable]
public enum EconomyBankAccountParam
{
    AccountName,
    Blocked,
    CanReachPayDay,
    JobName,
    Salary
}
