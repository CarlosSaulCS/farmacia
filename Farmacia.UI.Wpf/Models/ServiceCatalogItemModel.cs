using CommunityToolkit.Mvvm.ComponentModel;

namespace Farmacia.UI.Wpf.Models;

public partial class ServiceCatalogItemModel : ObservableObject
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DefaultPrice { get; set; }

    [ObservableProperty]
    private decimal _chargeAmount;

    partial void OnChargeAmountChanged(decimal value)
    {
        if (value < 0)
        {
            ChargeAmount = 0;
        }
    }

    public void ResetToDefault() => ChargeAmount = DefaultPrice;
}
