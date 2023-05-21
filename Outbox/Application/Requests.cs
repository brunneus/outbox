namespace Outbox.Application
{
    public record OrderRequest(IEnumerable<OrderItemRequest> Items);
    public record OrderItemRequest(string ProductName, decimal UnitPrice, decimal Discount, int Units);
}
