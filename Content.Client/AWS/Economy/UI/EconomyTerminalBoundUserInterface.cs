using Content.Shared.AWS.Economy;
using Content.Client.AWS.Economy.UI;

namespace Content.Client.AWS.Economy.UI;

public sealed class EconomyTerminalBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private EconomyTerminalMenu? _menu;

    public EconomyTerminalBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    public void OnPayPressed(ulong amount, string reason)
    {
        SendMessage(new EconomyTerminalMessage(amount, reason));
    }

    protected override void Open()
    {
        base.Open();

        _menu = new EconomyTerminalMenu(this);
        _menu.OnClose += Close;

        _menu.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Dispose();
    }
}
