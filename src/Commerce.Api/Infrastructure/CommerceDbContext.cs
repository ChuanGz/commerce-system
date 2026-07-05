using Commerce.Api.Modules.Catalog;
using Commerce.Api.Modules.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Api.Infrastructure;

public sealed class CommerceDbContext(DbContextOptions<CommerceDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderTimelineEntry> OrderTimeline => Set<OrderTimelineEntry>();

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

        modelBuilder.Entity<Order>(order =>
        {
            order.ToTable("Ordering_Orders");
            order.HasKey(item => item.Id);
            order.HasIndex(item => new { item.CustomerId, item.IdempotencyKey }).IsUnique();
            order.Property(item => item.CustomerId).HasMaxLength(100).IsRequired();
            order.Property(item => item.IdempotencyKey).HasMaxLength(100).IsRequired();
            order.Property(item => item.IntentHash).HasMaxLength(64).IsRequired();
            order.Property(item => item.Currency).HasMaxLength(3).IsRequired();
            order.Property(item => item.UnitPrice).HasPrecision(18, 2);
            order.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<OrderTimelineEntry>(entry =>
        {
            entry.ToTable("Ordering_Timeline");
            entry.HasKey(item => item.Id);
            entry.Property(item => item.Event).HasMaxLength(100).IsRequired();
            entry.Property(item => item.Detail).HasMaxLength(500).IsRequired();
            entry.HasOne(item => item.Order)
                .WithMany(order => order.Timeline)
                .HasForeignKey(item => item.OrderId);
        });
    }
}
