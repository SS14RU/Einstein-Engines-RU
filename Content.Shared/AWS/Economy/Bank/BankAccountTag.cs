using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy.Bank;

[Serializable, NetSerializable]
public enum BankAccountTag
{
    Department,
    Station,
    Personal
}
