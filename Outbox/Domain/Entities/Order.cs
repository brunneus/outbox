namespace Outbox.Entities
{
    public class Order
    {
        private List<OrderItem> _items;

        public Order()
        {
            _items = new List<OrderItem>();
            Status = OrderStatus.Submitted;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public OrderStatus Status { get; private set; }

        public ICollection<OrderItem> Items
        {
            get => _items;
            set
            {
                _items = value.ToList();
            }
        }

        public OrderItem GetItemWithHighestDiscount()
        {
            var item = Items
                .OrderByDescending(item => item.Discount)
                .First();

            return item;
        }

        internal void AddItem(OrderItem orderItem)
        {
            _items.Add(orderItem);
        }

        internal void SetAsPaid()
        {
            Status = OrderStatus.Paid;
        }
    }
}

