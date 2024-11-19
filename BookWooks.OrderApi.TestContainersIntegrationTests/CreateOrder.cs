namespace BookWooks.OrderApi.TestContainersIntegrationTests
{
    [Collection("Order Test Collection")]
    public class CreateOrder : ApiTestBase<Program, BookyWooksOrderDbContext>
    {
        private readonly HttpClient _client;
        private readonly ITestHarness _harness;

        public CreateOrder(CustomOrderTestFactory<Program> apiFactory) : base(apiFactory, apiFactory.DisposeAsync)
        {
            _client = apiFactory.CreateClient();
            _harness = apiFactory.Services.GetTestHarness();
        }

        [Fact]
        public async Task SuccesfullyCreateNewOrder()
        {
            var (customer, product) = await OrderTestHelper.SetupCustomerAndProductAsync(AddAsync<Customer>, AddAsync<Product>);
            var deliveryAddress = new Address("Test Street", "Test City", "Test Country", "Post Code");
            var command = OrderTestHelper.CreateOrderCommand(customer, product, deliveryAddress);

            var commandResult = await SendAsync(command);

            Assert.True(commandResult.IsSuccess);
        }

        [Fact]
        public async Task InvalidStreetName()
        {
            var (customer, product) = await OrderTestHelper.SetupCustomerAndProductAsync(AddAsync<Customer>, AddAsync<Product>);
            var deliveryAddress = new Address(null, "Test City", "Test Country", "Post Code");
            var command = OrderTestHelper.CreateOrderCommand(customer, product, deliveryAddress);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await SendAsync(command));
            Assert.Equal("street", exception.ParamName);
        }

        [Fact]
        public async Task PostCodeExceedsCharacterRestriction()
        {
            var (customer, product) = await OrderTestHelper.SetupCustomerAndProductAsync(AddAsync<Customer>, AddAsync<Product>);
            var deliveryAddress = new Address("Test Street", "Test City", "Test Country", "Post Code 12345678910");
            var command = OrderTestHelper.CreateOrderCommand(customer, product, deliveryAddress);

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await SendAsync(command));
            Assert.Equal("Post Code", exception.ParamName);
        }

        [Fact]
        public async Task PublishOrderCreated()
        {
            await _harness.Start();

            var (customer, product) = await OrderTestHelper.SetupCustomerAndProductAsync(AddAsync<Customer>, AddAsync<Product>);
            var deliveryAddress = new Address("Test Street", "Test City", "Test Country", "Post Code");
            var command = OrderTestHelper.CreateOrderCommand(customer, product, deliveryAddress);

            var commandResult = await SendAsync(command);

            var isEventPublished = await _harness.Sent.Any<OrderCreatedMessage>();
            isEventPublished.Should().BeTrue();

            var messageSent = await _harness.Sent.SelectAsync<OrderCreatedMessage>()
            .FirstOrDefault();

            Assert.Equal(command.CustomerId, messageSent?.Context.Message.customerId);

            await _harness.Stop();
        }

        [Fact]
        public async Task CompletePayment()
        {
            await _harness.Start();

            var endPointName = _harness.EndpointNameFormatter.Consumer<CompletePaymentCommandConsumer>();

            var command = new CompletePaymentCommand(
                CorrelationId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                orderTotal: 9.99M
            );

            await _harness.Bus.Publish(command);

            var isEventPublished = await _harness.Published.Any<CompletePaymentCommand>();
            isEventPublished.Should().BeTrue();

            var consumerHarness = _harness.GetConsumerHarness<CompletePaymentCommandConsumer>();

            var isEventConsumed = await consumerHarness.Consumed.Any<CompletePaymentCommand>(x =>
            {
                var message = x.Context.Message;
                return message.customerId == command.customerId;
            });

            isEventConsumed.Should().BeTrue();

            await _harness.Stop();
        }

        [Fact]
        public async Task PublishOrderCreatedMessage()
        {
           EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:order-created"));
           await _harness.Start();
           var x = _harness.GetConsumerEndpoint<OrderCreatedConsumer>(); 

            var orderItems = new List<OrderItemEventDto>
            {
                new OrderItemEventDto(Guid.NewGuid(), 1, true)
            };

            var message = new OrderCreatedMessage(
                orderId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                orderTotal: 9.99M,
                orderItems: orderItems
            );

            var endPointName = _harness.EndpointNameFormatter.Consumer<OrderCreatedConsumer>();
            


            await _harness.Bus.Send(message);

            //var sendEndpoint = await _harness.Bus.GetSendEndpoint(new Uri("queue:order-created"));
            //await sendEndpoint.Send(message);

            //Assert.True(await _harness.Consumed.Any<OrderCreatedMessage>());
            //Assert.True(await _harness.Consumed.Any<OrderCreatedConsumer>());

            var isEventPublished = await _harness.Sent.Any<OrderCreatedMessage>();
            isEventPublished.Should().BeTrue();

            var consumerHarness = _harness.GetConsumerHarness<OrderCreatedConsumer>();

            var isEventConsumed = await consumerHarness.Consumed.Any<OrderCreatedMessage>(x =>
            {
                var message = x.Context.Message;
                return message.customerId == message.customerId;
            });

            isEventConsumed.Should().BeTrue();

            await _harness.Stop();
        }
    }
}
