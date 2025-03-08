namespace Content.Shared.AWS.Economy
{
    /// <summary>
    /// Interface for components that hold money. Not to ones that can carry a link to the account (like a card).
    /// </summary>
    public interface IEconomyMoneyHolder
    {
        public ulong Balance { get; set; }
    }
}
