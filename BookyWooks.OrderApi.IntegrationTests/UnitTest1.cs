using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
using BookWooks.OrderApi.UseCases.Create;
using OrderItem = BookWooks.OrderApi.UseCases.Create.OrderItem;
namespace BookyWooks.OrderApi.IntegrationTests;

public class UnitTest1 : OrderApiBaseIntegrationTest
{
    private readonly HttpClient _client;
    public UnitTest1(OrderApiApplicationTestFactory<Program> apiFactory) : base(apiFactory)
    {
        _client = apiFactory.CreateClient();
    }
    [Fact]
    public async Task SuccesfullyCreateNewOrder()
    {
        //await AddAsync(new Order("1", "Chris Porter", GetPreconfiguredDeliveryAddresses(), CardTypeEnum.MasterCard, "001", "1212121212", "Chris Porter", DateTime.Now.AddYears(1)));
        // Create a new customer
        var customer = Customer.Create("Customer Name", "Unique Email");
        var product = Product.Create(Guid.NewGuid(), "Book 1", "Book URL", 9.99M);
        var product2 = Product.Create(Guid.NewGuid(), "Book 2", "Book URL", 5.99M);
        await AddAsync(customer); // Add the customer to the database
        await AddAsync(product); // Add the customer to the database
        await AddAsync(product2); // Add the customer to the database

        var orderItems = new List<OrderItem>()
                    {
                        new OrderItem(product.Id, product.Price, 1 ),
                        new OrderItem(product2.Id, product2.Price, 4 ),
                    };
        var deliveryAddress = new Address("Test Street", "Test City", "Test Country", "Test Post");
        var paymentDetails = new PaymentDetails("1234 5678 9012 3456", "Christopher", "12/23", "123", 1);
        var command = new CreateOrderCommand
      (
            OrderItems: orderItems,
            CustomerId: customer.Id,
            DeliveryAddress: deliveryAddress,
            PaymentDetails: paymentDetails
       );

        var commandCreated = await SendAsync(command);

        Assert.True(commandCreated.IsSuccess);
    }
}