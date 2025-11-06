namespace Farmacia.UI.Wpf.Models;

public class ProductSuggestionModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? InternalCode { get; set; }
}
