using Outbox.Data;
using Outbox.Services;

namespace Outbox
{
    public class OutboxPublisherBackgroundService : BackgroundService
    {
        private readonly OutboxDbContext _dbContext;
        private readonly IBrokerPublisherService _brokerPublisherService;
        private readonly ILogger<OutboxPublisherBackgroundService> _logger;
        private const int ONE_MINUTE_IN_MS = 60000;

        public OutboxPublisherBackgroundService(IServiceProvider serviceProvider, ILogger<OutboxPublisherBackgroundService> logger)
        {
            var scope = serviceProvider.CreateScope();

            _dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            _brokerPublisherService = scope.ServiceProvider.GetRequiredService<IBrokerPublisherService>();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var outboxToPublish = _dbContext
                        .Outbox
                        .Where(o => !o.WasPublished)
                        .ToList();

                    foreach (var outbox in outboxToPublish)
                    {
                        await _brokerPublisherService.PublishEventAsync(outbox.Event);
                        outbox.SetAsPublished();

                        await _dbContext.SaveChangesAsync(stoppingToken);
                    }

                    await Task.Delay(ONE_MINUTE_IN_MS, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error ocurred processing outbox");
                }
            }
        }
    }
}
