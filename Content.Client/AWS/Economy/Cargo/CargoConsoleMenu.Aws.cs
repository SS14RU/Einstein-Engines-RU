using Content.Client.Cargo.UI;
using Content.Shared.Store;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Localization;
using Robust.Shared.Prototypes;

namespace Content.Client.Cargo.UI;

public sealed partial class CargoConsoleMenu
{
    private CurrencyPrototype? _awsCurrency;
    private int _awsCurrentBankBalance;
    private Label? _awsBalanceLabel;

    partial void AwsOnMenuConstructed()
    {
        if (PointsLabel.Parent is BoxContainer box && box.ChildCount > 0 && box.GetChild(0) is Label label)
            _awsBalanceLabel = label;
    }

    private partial string AwsFormatPointCost(int cost, string defaultText)
    {
        if (_awsCurrency == null)
            return defaultText;

        var amountText = cost.ToString();
        var currencyName = Loc.GetString(_awsCurrency.DisplayName);
        return Loc.GetString("aws-economy-cargo-console-currency-amount", ("currency", currencyName), ("amount", amountText));
    }

    partial void AwsOnBankDataUpdated(string name, int points)
    {
        _awsCurrentBankBalance = points;
        UpdateBalanceLabel();
        PointsLabel.Text = AwsFormatPointCost(points, Loc.GetString("cargo-console-menu-points-amount", ("amount", points.ToString())));
    }

    public void AwsSetCurrency(string? currencyId)
    {
        if (!string.IsNullOrWhiteSpace(currencyId) &&
            _protoManager.TryIndex<CurrencyPrototype>(currencyId, out var currency))
        {
            _awsCurrency = currency;
        }
        else
        {
            _awsCurrency = null;
        }

        UpdateBalanceLabel();
        PointsLabel.Text = AwsFormatPointCost(_awsCurrentBankBalance, Loc.GetString("cargo-console-menu-points-amount", ("amount", _awsCurrentBankBalance.ToString())));
    }

    private void UpdateBalanceLabel()
    {
        if (_awsBalanceLabel == null)
            return;

        if (_awsCurrency == null)
        {
            _awsBalanceLabel.Text = Loc.GetString("cargo-console-menu-points-label");
            return;
        }

        var currencyName = Loc.GetString(_awsCurrency.DisplayName);
        _awsBalanceLabel.Text = Loc.GetString("aws-economy-cargo-console-balance-label", ("currency", currencyName));
    }
}
