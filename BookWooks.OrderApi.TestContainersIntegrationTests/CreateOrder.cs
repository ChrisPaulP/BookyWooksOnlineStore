using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using OrderItem = BookWooks.OrderApi.UseCases.Create.OrderItem;

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
            // Create a new customer
            var customer = Customer.Create("Customer Name", "Unique Email");
            var product = Product.Create("Book 1", "Book URL", 9.99M);
            await AddAsync(customer); // Add the customer to the database
            await AddAsync(product); // Add the customer to the database
            // Create order items
            var orderItems = new List<OrderItem>()
    {
        new OrderItem(product.Id, 9.99M, 1),
        new OrderItem(product.Id, 5.99M, 4)
    };

            // Create delivery address
            var deliveryAddress = new Address("Test Street", "Test City", "Test Country", "Post Code");

            // Create payment details
            var paymentDetails = new PaymentDetails("1234 5678 9012 3456", "Christopher", "12/23", "123", 1);

            // Create a command to create the order
            var command = new CreateOrderCommand(
                OrderItems: orderItems,
                CustomerId: customer.Id,
                DeliveryAddress: deliveryAddress,
                PaymentDetails: paymentDetails
            );

            // Send the command to create the order
            var commandResult = await SendAsync(command);

            // Assert that the command execution was successful
            Assert.True(commandResult.IsSuccess);
        }

    }
}