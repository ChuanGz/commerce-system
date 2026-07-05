using Commerce.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commerce.Api.Tests;

public sealed class CommerceApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseSetting("Commerce:ApiKey", "integration-test-key");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CommerceDbContext>>();
            services.AddDbContext<CommerceDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task<Guid> AddProductAsync(int stock = 10)
    {
        var id = Guid.NewGuid();
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        db.Products.Add(new Commerce.Api.Modules.Catalog.Product
        {
            Id = id,
            Name = "Test Product",
            UnitPrice = 42.50m,
            Currency = "USD",
            AvailableQuantity = stock,
            Version = 1,
            IsActive = true
        });
        await db.SaveChangesAsync();
        return id;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}
