namespace Commerce.Api.Modules.Catalog;

public sealed class Product
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public decimal UnitPrice { get; set; }
    public required string Currency { get; set; }
    public int AvailableQuantity { get; set; }
    public long Version { get; set; }
    public bool IsActive { get; set; }
}

public sealed record ProductResponse(
    Guid Id,
    string Name,
    decimal UnitPrice,
    string Currency,
    long Version,
    int AvailableQuantity);
