namespace BookWooks.OrderApi.TestContainersIntegrationTests;

public static class OrderTestHelper
{
    public static async Task<(CustomerResult, ProductResult)> SetupCustomerAndProductAsync(
    Func<Customer, Task> addCustomer, Func<Product, Task> addProduct)
    {
        var uniqueEmail = $"customer_{Guid.NewGuid()}@gmail.com";
        var customerValidation = Customer.CreateCustomer(Guid.NewGuid(),"Customer Name", uniqueEmail);
        var productValidation = Product.CreateProduct(Guid.NewGuid(),"Book", "Book 1", 9.99M, 2);

        var customerResult = await customerValidation.MatchAsync(
            async customer =>
            {
                await addCustomer(customer);
                return CustomerResult.Success(customer);
            },
            errors => CustomerResult.Fail(errors)
        );

        var productResult = await productValidation.MatchAsync(
            async product =>
            {
                await addProduct(product);
                return ProductResult.Success(product);
            },
             errors => ProductResult.Fail(errors)
        );
        return (customerResult, productResult);
    }

    public static List<CreateOrderItemCommand> CreateOrderItems(Product product)
    {
        return new List<CreateOrderItemCommand>
        {
            new CreateOrderItemCommand(product!.ProductId.Value, product.Name.Value, product.Description.Value, product.Price.Value, product.Quantity.Value),
            new CreateOrderItemCommand(product!.ProductId.Value, product.Name.Value, product.Description.Value, product.Price.Value, product.Quantity.Value)
        };
    }

    public static CreateOrderCommand CreateOrderCommand(CustomerId customerId, Product product)
        => new CreateOrderCommand
        (
            customerId.Value,
            new CreateAddressCommand("123 Main St", "Some City", "Some Country", "12345"),
            new CreatePaymentDetailsCommand("1234567890123456", "John Doe", "12/25", 1),
            CreateOrderItems(product)
        );
}
