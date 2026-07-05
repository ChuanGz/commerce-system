using Commerce.Api.Infrastructure;
using Commerce.Api.Modules.Catalog;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<CommerceDbContext>();
builder.Services.AddDbContext<CommerceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Commerce")
        ?? "Data Source=commerce.db"));

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

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.EnsureSeededAsync(db);
}

app.Run();

public partial class Program;
