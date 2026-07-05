using System.Net;
using System.Net.Http.Json;
using Commerce.Api.Infrastructure;
using Commerce.Api.Modules.Catalog;
using Commerce.Api.Modules.Ordering;

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

    [Fact]
    public async Task Checkout_is_durable_idempotent_and_traceable()
    {
        var productId = await factory.AddProductAsync();
        var request = new CheckoutRequest("customer-1", productId, 2, 1);
        using var first = await PostCheckoutAsync(request, "intent-1");
        var accepted = await first.Content.ReadFromJsonAsync<CheckoutResponse>();
        using var replay = await PostCheckoutAsync(request, "intent-1");
        var replayed = await replay.Content.ReadFromJsonAsync<CheckoutResponse>();
        using var timeline = await _client.SendAsync(
            CreateAuthorizedRequest(
                HttpMethod.Get,
                $"/api/orders/{accepted!.OrderId}/timeline"));
        var events = await timeline.Content.ReadFromJsonAsync<OrderTimelineResponse>();

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(OrderStatus.Confirmed, accepted.Status);
        Assert.False(accepted.IsReplay);
        Assert.Equal(accepted.OrderId, replayed!.OrderId);
        Assert.True(replayed.IsReplay);
        Assert.Equal(["OrderCreated", "PaymentConfirmed"],
            events!.Entries.Select(entry => entry.Event));
        Assert.All(events.Entries, entry => Assert.False(
            string.IsNullOrWhiteSpace(entry.CorrelationId)));
    }

    [Fact]
    public async Task Conflicting_idempotency_intent_is_rejected()
    {
        var productId = await factory.AddProductAsync();
        using var accepted = await PostCheckoutAsync(
            new CheckoutRequest("customer-2", productId, 1, 1),
            "intent-2");
        using var conflict = await PostCheckoutAsync(
            new CheckoutRequest("customer-2", productId, 2, 1),
            "intent-2");

        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
    }

    [Fact]
    public async Task Stale_version_and_insufficient_stock_are_rejected()
    {
        var productId = await factory.AddProductAsync(stock: 2);
        using var stale = await PostCheckoutAsync(
            new CheckoutRequest("customer-3", productId, 1, 99),
            "stale");
        using var insufficient = await PostCheckoutAsync(
            new CheckoutRequest("customer-3", productId, 3, 1),
            "insufficient");

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, insufficient.StatusCode);
    }

    [Fact]
    public async Task Payment_timeout_remains_visible_for_reconciliation()
    {
        var productId = await factory.AddProductAsync();
        using var response = await PostCheckoutAsync(
            new CheckoutRequest("customer-4", productId, 1, 1, "timeout"),
            "timeout-intent");
        var order = await response.Content.ReadFromJsonAsync<CheckoutResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(OrderStatus.PaymentUncertain, order!.Status);
    }

    [Fact]
    public async Task Checkout_without_api_key_is_rejected()
    {
        var productId = await factory.AddProductAsync();
        using var response = await _client.PostAsJsonAsync(
            "/api/checkouts",
            new CheckoutRequest("customer-5", productId, 1, 1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private Task<HttpResponseMessage> PostCheckoutAsync(
        CheckoutRequest request,
        string idempotencyKey)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/checkouts")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("Idempotency-Key", idempotencyKey);
        message.Headers.Add("X-API-Key", "integration-test-key");
        return _client.SendAsync(message);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        string path)
    {
        var message = new HttpRequestMessage(method, path);
        message.Headers.Add("X-API-Key", "integration-test-key");
        return message;
    }
}
