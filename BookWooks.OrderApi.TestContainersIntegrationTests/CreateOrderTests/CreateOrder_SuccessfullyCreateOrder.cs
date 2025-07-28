using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.TestContainersIntegrationTests.CreateOrderTests;

[Collection("Order Test Collection")]
public class CreateOrder_SuccessfullyCreateOrder
    : ApiTestBase<Program, BookyWooksOrderDbContext>
{
    private readonly HttpClient _client;

    public CreateOrder_SuccessfullyCreateOrder(CustomOrderTestFactory<Program> apiFactory)
        : base(apiFactory, apiFactory.DisposeAsync)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task SuccessfullyCreateNewOrder()
    {
        var (customerResult, productResult) =
            await OrderTestHelper.SetupCustomerAndProductAsync(AddAsync<Customer>, AddAsync<Product>);

        customerResult.IsSuccess.Should().BeTrue();
        productResult.IsSuccess.Should().BeTrue();

        var customerId = customerResult.Match(c => c.CustomerId, _ => CustomerId.New());
        var product = productResult.Match(p => (Product)p!, _ => null);
        product.Should().NotBeNull();

        var createOrderCommand = OrderTestHelper.CreateOrderCommand(customerId, product!);
        var orderResponse = await SendAsync(createOrderCommand);

        orderResponse.IsRight.Should().BeTrue();
        var orderId = orderResponse.Match(o => o, errors => throw new Exception(string.Join(", ", errors)));

        var order = await FindAsync<Order>(orderId);
        var orderItems = await FindByForeignKeyAsync<OrderItem>(oi => oi.OrderId == order!.OrderId);

        order.Should().NotBeNull();
        order!.CustomerId.Value.Should().Be(customerId.Value);
        orderItems.Should().HaveCount(createOrderCommand.OrderItems.Count());
    }
}