using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Outbox;
using Outbox.Application;
using Outbox.Data;
using Outbox.Entities;
using Outbox.Services;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IBrokerPublisherService, KafkaOrderMessageProducer>();
builder.Services.AddHostedService<DatabaseMigrationHostedService>();
builder.Services.AddHostedService<OutboxPublisherBackgroundService>();

var resourceBuilder = ResourceBuilder.CreateDefault().AddService("Outbox");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracingBuilder =>
    {
        tracingBuilder
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(e =>
            {
                e.EnrichWithIDbCommand = (activity, command) =>
                {
                    activity.AddTag("sqlCmd", command.CommandText);
                };
            })
            .AddOtlpExporter()
            .SetResourceBuilder(resourceBuilder);
    })
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            .AddMeter("outbox")
            .AddOtlpExporter()
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation();
    });

builder.Services.AddDbContext<OutboxDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
});

builder.Logging.ClearProviders();

builder.Logging.AddOpenTelemetry(loggerBuilder =>
{
    loggerBuilder
        .SetResourceBuilder(resourceBuilder)
        .AddOtlpExporter();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/order", async ([FromBody] OrderRequest request, [FromServices] OrderService orderService, [FromServices]ILogger<Program> logger) =>
{
    var order = new Order();

    foreach (var item in request.Items)
    {
        order.AddItem(new OrderItem(item.ProductName, item.UnitPrice, item.Discount, item.Units));
    }

    await orderService.AddOrderAsync(order);

    logger.LogInformation("Order with Id {OrderId} created", order.Id);

    var meter = new Meter(name: "outbox");
    var counter = meter.CreateCounter<int>("orders.counting", "Number of orders created");
    counter.Add(1);

    return order;
})
.WithName("CreateOrder");

app.MapPut("/order/{orderId}/pay", async (Guid orderId, [FromServices] OrderService orderService) =>
{
    var order = await orderService.SetOrderAsPaid(orderId);

    return order;
})
.WithName("SetOrderAsPaid");

app.Run();

public partial class Program
{
    protected Program() { }
}
