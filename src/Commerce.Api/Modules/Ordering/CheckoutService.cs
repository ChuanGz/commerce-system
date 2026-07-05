using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Commerce.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Api.Modules.Ordering;

public sealed class CheckoutService(CommerceDbContext db)
{
    public async Task<CheckoutResult> CheckoutAsync(
        CheckoutRequest request,
        string idempotencyKey,
        string correlationId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerId)
            || request.CustomerId.Length > 100
            || request.Quantity is < 1 or > 100
            || string.IsNullOrWhiteSpace(idempotencyKey)
            || idempotencyKey.Length > 100)
        {
            return CheckoutResult.Invalid("Invalid customer, quantity, or idempotency key.");
        }

        var intentHash = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))));
        var existing = await db.Orders.AsNoTracking().SingleOrDefaultAsync(
            order => order.CustomerId == request.CustomerId
                && order.IdempotencyKey == idempotencyKey,
            cancellationToken);
        if (existing is not null)
        {
            return existing.IntentHash == intentHash
                ? CheckoutResult.Success(ToResponse(existing, true))
                : CheckoutResult.Conflict("The idempotency key belongs to another intent.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(
            cancellationToken);
        var product = await db.Products.SingleOrDefaultAsync(
            item => item.Id == request.ProductId && item.IsActive,
            cancellationToken);
        if (product is null)
        {
            return CheckoutResult.NotFound("Product not found.");
        }

        if (product.Version != request.ExpectedProductVersion)
        {
            return CheckoutResult.Conflict("Product price or stock version is stale.");
        }

        if (product.AvailableQuantity < request.Quantity)
        {
            return CheckoutResult.Conflict("Insufficient stock.");
        }

        product.AvailableQuantity -= request.Quantity;
        product.Version++;
        var status = request.PaymentSimulation.Equals(
            "timeout", StringComparison.OrdinalIgnoreCase)
            ? OrderStatus.PaymentUncertain
            : OrderStatus.Confirmed;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            IdempotencyKey = idempotencyKey,
            IntentHash = intentHash,
            ProductId = product.Id,
            Quantity = request.Quantity,
            UnitPrice = product.UnitPrice,
            Currency = product.Currency,
            Status = status,
            CorrelationId = correlationId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        order.Timeline.Add(CreateEntry(order, "OrderCreated", "Stock reserved and order persisted."));
        order.Timeline.Add(CreateEntry(
            order,
            status == OrderStatus.Confirmed ? "PaymentConfirmed" : "PaymentUncertain",
            status == OrderStatus.Confirmed
                ? "Simulated payment accepted."
                : "Simulated payment timed out; reconciliation required."));
        db.Orders.Add(order);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return CheckoutResult.Conflict("Checkout conflicted with another accepted intent.");
        }

        return CheckoutResult.Success(ToResponse(order, false));
    }

    private static OrderTimelineEntry CreateEntry(Order order, string name, string detail) => new()
    {
        Order = order,
        Event = name,
        Detail = detail,
        CorrelationId = order.CorrelationId,
        OccurredAt = DateTimeOffset.UtcNow
    };

    private static CheckoutResponse ToResponse(Order order, bool replay) => new(
        order.Id,
        order.Status,
        order.UnitPrice,
        order.Currency,
        order.Quantity,
        replay);
}

public sealed record CheckoutResult(int StatusCode, CheckoutResponse? Value, string? Error)
{
    public static CheckoutResult Success(CheckoutResponse value) => new(200, value, null);
    public static CheckoutResult Invalid(string error) => new(400, null, error);
    public static CheckoutResult NotFound(string error) => new(404, null, error);
    public static CheckoutResult Conflict(string error) => new(409, null, error);
}
