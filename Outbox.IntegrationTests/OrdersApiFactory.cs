using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Outbox.Data;
using Xunit;

namespace Outbox.IntegrationTests
{
    public class OrdersApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly IContainer _postgresContainer = new ContainerBuilder()
            .WithImage("postgres:latest")
            .WithEnvironment("POSTGRES_PASSWORD", "weakPassword123")
            .WithPortBinding(5432, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextOptionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<OutboxDbContext>));

                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(OutboxDbContext));

                services.Remove(dbContextDescriptor!);
                services.Remove(dbContextOptionsDescriptor!);

                services.AddDbContext<OutboxDbContext>(options =>
                    options.UseNpgsql($"Host=localhost;Port=5432;Database=Orders;Username=postgres;Password=weakPassword123"));
            });
        }

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.StopAsync();
        }
    }
}
