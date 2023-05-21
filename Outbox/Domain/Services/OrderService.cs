using Outbox.Data;
using Outbox.Domain.Entities;
using Outbox.Entities;
using Outbox.Events;

namespace Outbox.Services
{
    public class OrderService
    {
        private readonly OutboxDbContext _context;
        private readonly IBrokerPublisherService _brokerPublisherService;

        public OrderService(OutboxDbContext context, IBrokerPublisherService brokerPublisherService)
        {
            _context = context;
            _brokerPublisherService = brokerPublisherService;
        }

        public async Task AddOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order> SetOrderAsPaid(Guid orderId)
        {
            var order = _context
                .Orders
                .FirstOrDefault(order => order.Id == orderId) ?? throw new Exception($"Order {orderId} not found");

            var transaction = await _context.Database.BeginTransactionAsync();

            order.SetAsPaid();

            var outbox = new OutboxRecord(new OrderPaidEvent(order));
            _context.Outbox.Add(outbox);

            _context.SaveChanges();

            await transaction.CommitAsync();

            return order;
        }
    }
}
