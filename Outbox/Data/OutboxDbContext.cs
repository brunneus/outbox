using Microsoft.EntityFrameworkCore;
using Outbox.Domain.Entities;
using Outbox.Entities;
using System.Text.Json;

namespace Outbox.Data
{
    public class OutboxDbContext : DbContext
    {
        public OutboxDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Order>()
                .HasMany(e => e.Items)
                .WithOne()
                .IsRequired();

            modelBuilder
                .Entity<Order>()
                .Navigation(e => e.Items)
                .AutoInclude();

            modelBuilder.Entity<OutboxRecord>()
                .Property(e => e.Event)
                .HasConversion(
                    @event => JsonSerializer.Serialize(@event, new JsonSerializerOptions()),
                    serialized => JsonSerializer.Deserialize<object>(serialized, new JsonSerializerOptions())!
                );
        }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OutboxRecord> Outbox { get; set; }
    }
}
