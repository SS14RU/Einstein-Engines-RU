using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy;

[Serializable, NetSerializable]
public sealed class EconomyTerminalMessage(ulong amount, string reason) : BoundUserInterfaceMessage
{
    public readonly ulong Amount = amount;
    public readonly string Reason = reason;
}
[Serializable, NetSerializable]
public enum EconomyTerminalUiKey
{
    Key
}
