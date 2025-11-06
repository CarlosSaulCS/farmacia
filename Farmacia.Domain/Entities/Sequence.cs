namespace Farmacia.Domain.Entities;

public class Sequence : EntityBase
{
    public required string Name { get; set; }
    public long CurrentValue { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public int Padding { get; set; } = 6;
}
