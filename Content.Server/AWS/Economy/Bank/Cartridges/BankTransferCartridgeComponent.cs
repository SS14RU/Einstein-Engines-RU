namespace Content.Server.AWS.Economy.Bank.Cartridges;

[RegisterComponent]
public sealed partial class BankTransferCartridgeComponent : Component
{
    /// <summary>
    /// Remembers the loader this cartridge is currently associated with.
    /// </summary>
    public EntityUid? Loader;

    /// <summary>
    /// Error message that should be displayed on the next UI state update.
    /// </summary>
    public string? PendingError;
}
