using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace BookWooks.OrderApi.TestContainersIntegrationTests.CreateOrderTests;
//[Collection("Order Test Collection")]
//public class CreateOrder_PublishOrderCreated
//    : ApiTestBase<Program, BookyWooksOrderDbContext>
//{
//    private readonly ITestHarness Harness;
//    public CreateOrder_PublishOrderCreated(CustomOrderTestFactory<Program> apiFactory)
//        : base(apiFactory, apiFactory.DisposeAsync)
//    {
//        Harness = apiFactory.Services.GetTestHarness();
//    }

//    [Fact]
//    public async Task PublishOrderCreated()
//    {
//        using var scope = _apiFactory.Services.CreateScope();
//        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();

//        await harness.Start();

//        var (customerResult, productResult) =
//            await OrderTestHelper.SetupCustomerAndProductAsync(AddAsync<Customer>, AddAsync<Product>);

//        customerResult.IsSuccess.Should().BeTrue();
//        productResult.IsSuccess.Should().BeTrue();

//        var customerId = customerResult.Match(c => c.CustomerId, _ => CustomerId.New());
//        var product = productResult.Match(p => (Product)p!, _ => null);
//        product.Should().NotBeNull();

//        var createOrderCommand = OrderTestHelper.CreateOrderCommand(customerId, product!);
//        var commandResult = await SendAsync(createOrderCommand);

//        var isEventPublished = await Harness.Sent.Any<OrderCreatedMessage>();
//        isEventPublished.Should().BeTrue();

//        var messageSent = await Harness.Sent.SelectAsync<OrderCreatedMessage>().FirstOrDefault();
//        Assert.Equal(createOrderCommand.CustomerId, messageSent?.Context.Message.customerId);
//        await harness.Stop();
//    }
//}

