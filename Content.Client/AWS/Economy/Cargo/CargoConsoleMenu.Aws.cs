using Content.Shared.Store;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Localization;
using Robust.Shared.Prototypes;

namespace Content.Client.Cargo.UI;

public sealed partial class CargoConsoleMenu
{
    private CurrencyPrototype? _awsCurrency;
    private int _awsCurrentBankBalance;
    private Label? _awsPointsKeyLabel;

    partial void OnMenuConstructed()
    {
        _awsPointsKeyLabel = PointsKeyLabel;
        UpdatePointsKeyLabel();
    }

    private partial string FormatPointCost(int cost, string defaultText)
    {
        if (_awsCurrency == null)
            return defaultText;

        var amountText = cost.ToString();
        var currencyName = Loc.GetString(_awsCurrency.DisplayName);
        return Loc.GetString("aws-economy-cargo-console-currency-amount", ("currency", currencyName), ("amount", amountText));
    }

    partial void OnBankDataUpdated(string name, int points)
    {
        _awsCurrentBankBalance = points;
        PointsLabel.Text = FormatPointCost(points, Loc.GetString("cargo-console-menu-points-amount", ("amount", points.ToString())));
        UpdatePointsKeyLabel();
    }

    public void SetCurrency(string? currencyId)
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

        PointsLabel.Text = FormatPointCost(_awsCurrentBankBalance, Loc.GetString("cargo-console-menu-points-amount", ("amount", _awsCurrentBankBalance.ToString())));
        UpdatePointsKeyLabel();
    }

    private void UpdatePointsKeyLabel()
    {
        if (_awsPointsKeyLabel == null)
            return;

        if (_awsCurrency == null)
        {
            _awsPointsKeyLabel.Text = Loc.GetString("cargo-console-menu-points-label");
            return;
        }

        var currencyName = Loc.GetString(_awsCurrency.DisplayName);
        _awsPointsKeyLabel.Text = Loc.GetString("aws-economy-cargo-console-balance-label", ("currency", currencyName));
    }
}
