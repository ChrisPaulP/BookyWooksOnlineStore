namespace BookWooks.OrderApi.TestContainersIntegrationTests;

[Collection("Order Test Collection")]
public class CreateOrderTests : ApiTestBase<Program, BookyWooksOrderDbContext>
{
    private readonly HttpClient _client;
    private readonly ITestHarness _harness;

    public CreateOrderTests(CustomOrderTestFactory<Program> apiFactory) : base(apiFactory, apiFactory.DisposeAsync)
    {
        _client = apiFactory.CreateClient();
        _harness = apiFactory.Services.GetTestHarness();
    }

    [Fact]
    public async Task SuccessfullyCreateNewOrder()
    {
        //System.Diagnostics.Debugger.Launch();
        var (customerResult, productResult) = await OrderTestHelper.SetupCustomerAndProductAsync(AddAsync<Customer>, AddAsync<Product>);
        // Assert: Ensure customer and product creation succeeded
        customerResult.IsSuccess.Should().BeTrue("Customer creation should succeed");
        productResult.IsSuccess.Should().BeTrue("Product creation should succeed");

        var customerId = customerResult.Match(
            customer => customer.CustomerId,
            errors => CustomerId.New()
        );

        var product = productResult.Match(
            product => (Product?)product,
            errors => null
        );

        product.Should().NotBeNull("Product creation should succeed");

        var createOrderCommand = OrderTestHelper.CreateOrderCommand(customerId, product!);

        var orderResponse = await SendAsync(createOrderCommand);

        var orderId = orderResponse.Match(
            orderId => orderId,
            errors => throw new Exception(string.Join(", ", errors))

        );
        orderResponse.IsRight.Should().BeTrue();

        orderId.Should().NotBeNull();
        orderId.Should().BeOfType<OrderId>();
        orderId.Value.Should().NotBeEmpty();
        orderId.Should().NotBeNull("order creation should succeed");


        // Act: Verify order persistence
        var order = await FindAsync<Order>(orderId);
        var orderItems = await FindByForeignKeyAsync<OrderItem>(oi => oi.OrderId == order!.OrderId);
        order.Should().NotBeNull("the order should be persisted in the database");

        // Assert: Check order properties
        order!.CustomerId.Value.Should().Be(customerId.Value);
        order.DeliveryAddress.Street.Value.Should().Be(createOrderCommand.DeliveryAddress.Street);
        order.Payment.CardName.Value.Should().Be(createOrderCommand.PaymentDetails.CardHolderName);
        orderItems.Should().HaveCount(createOrderCommand.OrderItems.Count());
    }
    //[Fact]
    //public async Task PublishOrderCreated()
    //{
    //    await _harness.Start();

    //    var (customerResult, productResult) = await OrderTestHelper.SetupCustomerAndProductAsync(AddAsync<Customer>, AddAsync<Product>);

    //    // Assert: Ensure customer and product creation succeeded
    //    customerResult.IsSuccess.Should().BeTrue("Customer creation should succeed");
    //    productResult.IsSuccess.Should().BeTrue("Product creation should succeed");

    //    var customerId = customerResult.Match(
    //        customer => customer.CustomerId,
    //        errors => CustomerId.New()
    //    );

    //    var product = productResult.Match(
    //        product => (Product?)product,
    //        errors => null
    //    );

    //    product.Should().NotBeNull("Product creation should succeed");
    //    var createOrderCommand = OrderTestHelper.CreateOrderCommand(customerId, product!);

    //    var commandResult = await SendAsync(createOrderCommand);

    //    var isEventPublished = await _harness.Sent.Any<OrderCreatedMessage>();
    //    isEventPublished.Should().BeTrue();

    //    var messageSent = await _harness.Sent.SelectAsync<OrderCreatedMessage>()
    //    .FirstOrDefault();

    //    Assert.Equal(createOrderCommand.CustomerId, messageSent?.Context.Message.customerId);

    //    await _harness.Stop();
    //}
    [Fact]
    public async Task CompletePayment()
    {
        await _harness.Start();

        var endPointName = _harness.EndpointNameFormatter.Consumer<CompletePaymentCommandConsumer>();

        var command = new CompletePaymentCommand(
            CorrelationId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderTotal: 9.99M
        );

        await _harness.Bus.Publish(command);

        var isEventPublished = await _harness.Published.Any<CompletePaymentCommand>();
        isEventPublished.Should().BeTrue();

        var consumerHarness = _harness.GetConsumerHarness<CompletePaymentCommandConsumer>();

        var isEventConsumed = await consumerHarness.Consumed.Any<CompletePaymentCommand>(x =>
        {
            var message = x.Context.Message;
            return message.CustomerId == command.CustomerId;
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
