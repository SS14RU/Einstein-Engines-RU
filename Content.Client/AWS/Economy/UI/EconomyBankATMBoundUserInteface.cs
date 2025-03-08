using Content.Shared.AWS.Economy;
using Content.Client.AWS.Economy.UI;

namespace Content.Client.AWS.Economy.UI;

public sealed class EconomyBankATMBoundUserInteface : BoundUserInterface
{
    [ViewVariables]
    private EconomyBankATMMenu? _menu;
    private EconomyBankATMAccountInfo? _bankAccount;

    public EconomyBankATMBoundUserInteface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    public void OnWithdrawPressed(ulong amount)
    {
        if (_bankAccount is null)
            return;
        SendMessage(new EconomyBankATMWithdrawMessage(amount));
    }

    public void OnTransferPressed(ulong amount, string recipientId)
    {
        if (_bankAccount is null)
            return;
        SendMessage(new EconomyBankATMTransferMessage(amount, recipientId));
    }

    protected override void Open()
    {
        base.Open();

        _menu = new EconomyBankATMMenu(this);
        _menu.OnClose += Close;

        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not EconomyBankATMUserInterfaceState atmState)
            return;

        _bankAccount = atmState.BankAccount;

        _menu?.SetBankAcount(atmState.BankAccount);
        _menu?.SetError(atmState.Error);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Dispose();
    }
}
