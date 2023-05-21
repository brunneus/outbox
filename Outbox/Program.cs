using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outbox;
using Outbox.Application;
using Outbox.Data;
using Outbox.Entities;
using Outbox.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IBrokerPublisherService, KafkaOrderMessageProducer>();
builder.Services.AddHostedService<DatabaseMigrationHostedService>();
builder.Services.AddHostedService<OutboxPublisherBackgroundService>();

builder.Services.AddDbContext<OutboxDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/order", async ([FromBody]OrderRequest request, [FromServices]OrderService orderService) =>
{
    var order = new Order();

    foreach(var item in request.Items)
    {
        order.AddItem(new OrderItem(item.ProductName, item.UnitPrice, item.Discount, item.Units));
    }

    await orderService.AddOrderAsync(order);

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
