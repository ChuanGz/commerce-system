namespace Commerce.Api.Modules.Ordering;

public enum OrderStatus
{
    PendingPayment,
    Confirmed,
    PaymentUncertain,
    Cancelled
}

public sealed class Order
{
    public Guid Id { get; set; }
    public required string CustomerId { get; set; }
    public required string IdempotencyKey { get; set; }
    public required string IntentHash { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public required string Currency { get; set; }
    public OrderStatus Status { get; set; }
    public required string CorrelationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<OrderTimelineEntry> Timeline { get; set; } = [];
}

public sealed class OrderTimelineEntry
{
    public long Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public required string Event { get; set; }
    public required string Detail { get; set; }
    public required string CorrelationId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed record CheckoutRequest(
    string CustomerId,
    Guid ProductId,
    int Quantity,
    long ExpectedProductVersion,
    string PaymentSimulation = "success");

public sealed record CheckoutResponse(
    Guid OrderId,
    OrderStatus Status,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    bool IsReplay);

public sealed record TimelineEntryResponse(
    string Event,
    string Detail,
    string CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record OrderTimelineResponse(
    Guid OrderId,
    OrderStatus Status,
    IReadOnlyList<TimelineEntryResponse> Entries);
