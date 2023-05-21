namespace Outbox.Entities
{
    public class OrderItem
    {
        public OrderItem(string productName, decimal unitPrice, decimal discount, int units)
        {
            ProductName = productName;
            UnitPrice = unitPrice;
            Discount = discount;
            Units = units;
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public string ProductName { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Discount { get; private set; }
        public int Units { get; private set; }
    }
}
