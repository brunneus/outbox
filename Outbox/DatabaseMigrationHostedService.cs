using Microsoft.EntityFrameworkCore;
using Outbox.Data;

namespace Outbox
{
    public class DatabaseMigrationHostedService : IHostedService
    {

        private readonly OutboxDbContext _dbContext;

        public DatabaseMigrationHostedService(IServiceProvider serviceProvider, ILogger<OutboxPublisherBackgroundService> logger)
        {
            var scope = serviceProvider.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _dbContext.Database.MigrateAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
