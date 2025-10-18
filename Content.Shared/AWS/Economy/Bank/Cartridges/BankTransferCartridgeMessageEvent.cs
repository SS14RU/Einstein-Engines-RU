using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy.Bank.Cartridges;

[Serializable, NetSerializable]
public sealed class BankTransferCartridgeUiMessageEvent : CartridgeMessageEvent
{
    public readonly ulong Amount;
    public readonly string RecipientAccountId;

    public BankTransferCartridgeUiMessageEvent(ulong amount, string recipientAccountId)
    {
        Amount = amount;
        RecipientAccountId = recipientAccountId;
    }
}
