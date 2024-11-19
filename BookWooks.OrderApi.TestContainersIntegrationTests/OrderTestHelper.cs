namespace BookWooks.OrderApi.TestContainersIntegrationTests;

public static class OrderTestHelper
{
    public static async Task<(Customer, Product)> SetupCustomerAndProductAsync(Func<Customer, Task> addCustomer, Func<Product, Task> addProduct)
    {
        var uniqueEmail = $"customer_{Guid.NewGuid()}@example.com";
        var customer = Customer.Create("Customer Name", uniqueEmail);
        var product = Product.Create(Guid.NewGuid(), "Book 1", "Book URL", 9.99M);
        await addCustomer(customer);
        await addProduct(product);
        return (customer, product);
    }

    public static List<OrderItem> CreateOrderItems(Guid productId)
    {
        return new List<OrderItem>
        {
            new OrderItem(productId, 9.99M, 1),
            new OrderItem(productId, 5.99M, 4)
        };
    }

    public static PaymentDetails CreatePaymentDetails()
    {
        return new PaymentDetails("1234 5678 9012 3456", "Christopher", "12/23", "123", 1);
    }

    public static CreateOrderCommand CreateOrderCommand(Customer customer, Product product, Address address)
    {
        var orderItems = CreateOrderItems(product.Id);
        var paymentDetails = CreatePaymentDetails();
        return new CreateOrderCommand(
            OrderItems: orderItems,
            CustomerId: customer.Id,
            DeliveryAddress: address,
            PaymentDetails: paymentDetails
        );
    }
}
