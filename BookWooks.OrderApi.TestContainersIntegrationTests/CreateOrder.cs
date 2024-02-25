namespace BookWooks.OrderApi.TestContainersIntegrationTests
{
    public class CreateOrder : OrderApiBaseIntegrationTest
    {
        private readonly HttpClient _client;
        public CreateOrder(OrderApiApplicationFactory<Program> apiFactory) : base(apiFactory)
        {
                _client = apiFactory.CreateClient();
        }
        [Fact]
        public async Task SuccesfullyCreateNewOrder()
        {
            //await AddAsync(new Order("1", "Chris Porter", GetPreconfiguredDeliveryAddresses(), CardTypeEnum.MasterCard, "001", "1212121212", "Chris Porter", DateTime.Now.AddYears(1)));

            var orderItems = new List<OrderCommandOrderItem>()
                    {
                        new OrderCommandOrderItem(1, "Test", 2.9M, 6, "bookimageurl" ),
                        new OrderCommandOrderItem(2, "Test2", 4.9M, 3, "bookimageurl" )
                    };

            var command = new CreateOrderCommand
          (
               Guid.NewGuid(),
               orderItems,
               "1",
               "Thomas",
               "Integration Test",
               "Belfast",
               "Integration Test",
               "Integration Test",
               "Integration Test",
               "Integration Test",
                DateTime.Now.AddYears(5),
               "Integration Test"
           );

            var commandCreated = await SendAsync(command);

            Assert.True(commandCreated.IsSuccess);
        }
    }
}