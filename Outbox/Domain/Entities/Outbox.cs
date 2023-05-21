namespace Outbox.Domain.Entities
{
    public class OutboxRecord
    {
        public OutboxRecord(object @event)
        {
            Event = @event;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public bool WasPublished { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public object Event { get; private set; }

        public void SetAsPublished()
        {
            WasPublished = true;
            PublishedAt = DateTime.UtcNow;
        }
    }
}
