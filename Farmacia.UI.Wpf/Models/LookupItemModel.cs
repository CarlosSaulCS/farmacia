namespace Farmacia.UI.Wpf.Models;

public class LookupItemModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}
