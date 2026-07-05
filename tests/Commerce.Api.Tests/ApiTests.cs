using System.Net;
using System.Net.Http.Json;
using Commerce.Api.Infrastructure;
using Commerce.Api.Modules.Catalog;

namespace Commerce.Api.Tests;

public sealed class ApiTests(CommerceApiFactory factory)
    : IClassFixture<CommerceApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_checks_database_and_returns_correlation_id()
    {
        using var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task Product_query_returns_price_version_and_stock()
    {
        using var response = await _client.GetAsync(
            $"/api/products/{SeedData.ReferenceProductId}");
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(product);
        Assert.Equal(25.00m, product.UnitPrice);
        Assert.Equal("USD", product.Currency);
        Assert.Equal(1, product.Version);
        Assert.Equal(100, product.AvailableQuantity);
    }

    [Fact]
    public async Task Unknown_product_is_not_found()
    {
        using var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
