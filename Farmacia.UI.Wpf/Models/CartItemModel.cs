namespace Farmacia.UI.Wpf.Models;

public class CartItemModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal => (UnitPrice - Discount) * Quantity * (1 + TaxRate);
}
