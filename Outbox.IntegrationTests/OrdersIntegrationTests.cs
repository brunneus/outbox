using FluentAssertions;
using Newtonsoft.Json;
using Outbox.Application;
using System.Text;
using Xunit;

namespace Outbox.IntegrationTests
{
    public class OrdersIntegrationTests : IClassFixture<OrdersApiFactory>
    {
        private readonly OrdersApiFactory _ordersApiFactory;

        public OrdersIntegrationTests(OrdersApiFactory ordersApiFactory)
        {
            _ordersApiFactory = ordersApiFactory;
        }

        [Fact]
        public async Task ShouldCreateOrderCorrectly()
        {
            var apiFactoryClient = _ordersApiFactory.CreateClient();

            var orderItems = new OrderItemRequest[]
            {
                new OrderItemRequest(ProductName: "chair", UnitPrice: 150, Discount: 10, Units: 3)
            };

            var body = new OrderRequest(orderItems);

            var requestContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var result = await apiFactoryClient.PostAsync("/order", requestContent);

            result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
