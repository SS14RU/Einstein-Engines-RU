using Content.Shared.AWS.Economy;
using Content.Shared.Containers.ItemSlots;

namespace Content.Client.AWS.Economy.UI.ManagementConsole;

public sealed class EconomyManagementConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private EconomyManagementConsoleMenu? _menu;

    public EconomyManagementConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _menu = new EconomyManagementConsoleMenu(this);
        _menu.OnClose += Close;

        _menu.PrivilegedIdButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(EconomyManagementConsoleComponent.ConsoleCardID));
        _menu.TargetIdButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(EconomyManagementConsoleComponent.TargetCardID));
        _menu?.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _menu?.Dispose();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not EconomyManagementConsoleUserInterfaceState consoleState)
            return;

        _menu?.UpdateState(consoleState);
    }

    public void BlockAccountToggle(EconomyBankAccountComponent? account)
    {
        if (account is null)
            return;

        var blocked = !account.Blocked;
        var msg = new EconomyManagementConsoleChangeParameterMessage(account.AccountID, EconomyBankAccountParam.Blocked, blocked);

        SendMessage(msg);
    }

    public void ChangeName(EconomyBankAccountComponent? account, string newName)
    {
        // reeeee hardcoding
        if (account is null || newName.Length > 40)
            return;

        var msg = new EconomyManagementConsoleChangeParameterMessage(account.AccountID, EconomyBankAccountParam.AccountName, newName);
        SendMessage(msg);
    }

    public void ChangeJob(EconomyBankAccountComponent? account, string jobName)
    {
        if (account is null)
            return;

        var msg = new EconomyManagementConsoleChangeParameterMessage(account.AccountID, EconomyBankAccountParam.JobName, jobName);
        SendMessage(msg);
    }

    public void ChangeSalary(EconomyBankAccountComponent? account, ulong salary)
    {
        if (account is null)
            return;

        var msg = new EconomyManagementConsoleChangeParameterMessage(account.AccountID, EconomyBankAccountParam.Salary, salary);
        SendMessage(msg);
    }

    public void ChangeAccountHolderID(NetEntity holder, string newID)
    {
        var msg = new EconomyManagementConsoleChangeHolderIDMessage(holder, newID);
        SendMessage(msg);
    }

    public void InitializeAccountOnHolder(NetEntity holder)
    {
        var msg = new EconomyManagementConsoleInitAccountOnHolderMessage(holder);
        SendMessage(msg);
    }

    public void PayBonus(string payer, float bonusPercent, List<string> accounts)
    {
        var msg = new EconomyManagementConsolePayBonusMessage(payer, bonusPercent, accounts);
        SendMessage(msg);
    }
}
