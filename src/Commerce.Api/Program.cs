using Commerce.Api.Infrastructure;
using Commerce.Api.Modules.Catalog;
using Commerce.Api.Modules.Ordering;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<CommerceDbContext>();
builder.Services.AddDbContext<CommerceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Commerce")
        ?? "Data Source=commerce.db"));
builder.Services.AddScoped<CheckoutService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
    correlationId = string.IsNullOrWhiteSpace(correlationId)
        ? Guid.NewGuid().ToString("N")
        : correlationId;
    context.TraceIdentifier = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    await next();
});

app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");

app.MapGet("/api/products/{id:guid}", async (
    Guid id,
    CommerceDbContext db,
    CancellationToken cancellationToken) =>
{
    var product = await db.Products
        .AsNoTracking()
        .Where(product => product.Id == id && product.IsActive)
        .Select(product => new ProductResponse(
            product.Id,
            product.Name,
            product.UnitPrice,
            product.Currency,
            product.Version,
            product.AvailableQuantity))
        .SingleOrDefaultAsync(cancellationToken);

    return product is null ? Results.NotFound() : Results.Ok(product);
})
.WithName("GetProduct")
.Produces<ProductResponse>()
.ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/api/checkouts", async (
    CheckoutRequest request,
    HttpContext context,
    CheckoutService checkout,
    CancellationToken cancellationToken) =>
{
    if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var key))
    {
        return Results.Problem("Idempotency-Key header is required.", statusCode: 400);
    }

    var result = await checkout.CheckoutAsync(
        request,
        key.ToString(),
        context.TraceIdentifier,
        cancellationToken);
    return result.Value is not null
        ? Results.Ok(result.Value)
        : Results.Problem(result.Error, statusCode: result.StatusCode);
})
.WithName("Checkout")
.Produces<CheckoutResponse>()
.ProducesProblem(400)
.ProducesProblem(404)
.ProducesProblem(409);

app.MapGet("/api/orders/{id:guid}/timeline", async (
    Guid id,
    CommerceDbContext db,
    CancellationToken cancellationToken) =>
{
    var order = await db.Orders
        .AsNoTracking()
        .Include(item => item.Timeline)
        .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
    return order is null
        ? Results.NotFound()
        : Results.Ok(new OrderTimelineResponse(
            order.Id,
            order.Status,
            order.Timeline.OrderBy(item => item.OccurredAt)
                .Select(item => new TimelineEntryResponse(
                    item.Event,
                    item.Detail,
                    item.CorrelationId,
                    item.OccurredAt))
                .ToList()));
})
.WithName("GetOrderTimeline")
.Produces<OrderTimelineResponse>()
.ProducesProblem(404);

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.EnsureSeededAsync(db);
}

app.Run();

public partial class Program;
