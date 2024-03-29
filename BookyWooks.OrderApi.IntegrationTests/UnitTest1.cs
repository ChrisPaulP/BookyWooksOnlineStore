using BookWooks.OrderApi.UseCases.Create;

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

        var orderItems = new List<OrderItem>()
                    {
                        new OrderItem(Guid.NewGuid(), 9.99M, 1 ),
                        new OrderItem(Guid.NewGuid(), 5.99M, 4 ),
                    };
        var DeliveryAddress = new Address("Test Street", "Test City", "Test Country", "Test Post Code");
        var PaymentDetails = new PaymentDetails("1234 5678 9012 3456", "Christopher", "12/23", "123", 1);
        var command = new CreateOrderCommand
      (
           orderItems,
           Guid.NewGuid(),
           DeliveryAddress,
           PaymentDetails
       );

        var commandCreated = await SendAsync(command);

        Assert.True(commandCreated.IsSuccess);
    }
}