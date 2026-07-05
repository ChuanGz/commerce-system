using Commerce.Api.Modules.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Api.Infrastructure;

public static class SeedData
{
    public static readonly Guid ReferenceProductId =
        Guid.Parse("9d67986b-9500-4527-858a-3118d1ac6a90");

    public static async Task EnsureSeededAsync(CommerceDbContext db)
    {
        if (await db.Products.AnyAsync())
        {
            return;
        }

        db.Products.Add(new Product
        {
            Id = ReferenceProductId,
            Name = "Reference Product",
            UnitPrice = 25.00m,
            Currency = "USD",
            AvailableQuantity = 100,
            Version = 1,
            IsActive = true
        });
        await db.SaveChangesAsync();
    }
}
