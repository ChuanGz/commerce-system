using Commerce.Api.Modules.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Api.Infrastructure;

public sealed class CommerceDbContext(DbContextOptions<CommerceDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.ToTable("Catalog_Products");
            product.HasKey(item => item.Id);
            product.Property(item => item.Name).HasMaxLength(200).IsRequired();
            product.Property(item => item.UnitPrice).HasPrecision(18, 2);
            product.Property(item => item.Currency).HasMaxLength(3).IsRequired();
            product.Property(item => item.Version).IsConcurrencyToken();
        });
    }
}
