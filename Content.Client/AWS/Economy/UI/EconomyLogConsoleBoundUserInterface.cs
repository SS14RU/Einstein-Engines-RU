namespace Content.Client.AWS.Economy.UI;

public sealed class EconomyLogConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private EconomyLogConsoleMenu? _menu;

    public EconomyLogConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _menu = new EconomyLogConsoleMenu(this);
        _menu.OnClose += Close;

        _menu?.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _menu?.Dispose();
    }
}
