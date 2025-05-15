public class SeedData 
{ 
public static class OrderSeedData
{
  public record OrderSeed(Guid CustomerId,string Street,string City,string Country,string PostCode,string CardHolderName,string CardNumber,string Expiration,int PaymentMethod);
  public record OrderItemSeedData(Guid OrderId, Guid ProductId, decimal Price, string ProductName, string ProductDescription, int Quantity);
  
  private static readonly OrderSeed _orderSeedData = new (Guid.NewGuid(), "123 Test St", "Testville", "Testland", "TST123", "Test User", "4111111111111111", "12/30", 1);
    //private static readonly OrderItemSeedData _orderItemSeedData = new(Guid.NewGuid(), Guid.NewGuid(), 49.99m, "Valid Product", "This is a valid product description.", 3);

    //public static IEnumerable<Order> CreateOrders(Guid customerId, Product product)
    //{
    //  return new[]
    //      {
    //      TryCreateOrder(customerId),
    //      //TryCreateOrderWithItems(customerId, product)
    //    }
    //      .Bind(validation => validation.Match(
    //          order => new[] { order },
    //          errors => Enumerable.Empty<Order>()
    //      ));
    //}
    public static IEnumerable<Order> CreateOrders(Guid customerId, Product product)
    {
      var validations = new[]
      {
        TryCreateOrder(customerId),
        TryCreateOrderWithItems(customerId, product)
    };

      foreach (var validation in validations)
      {
        if (validation.IsFail)
        {
          Console.WriteLine($"Validation failed:");
        }
      }

      return validations.Bind(validation => validation.Match(
    order =>
    {
      Console.WriteLine($"✅ Valid Order Created: {order.OrderId}");
      return [order];
    },
    errors =>
    {
      Console.WriteLine("❌ Validation failed! Orders will be empty.");
      return Enumerable.Empty<Order>();
    }
));
    }
    public static Validation<OrderValidationErrors, Order> TryCreateOrder(Guid customerId) => CreateOrder(customerId);
  public static Validation<OrderValidationErrors, Order> TryCreateOrderWithItems(Guid customerId, Product product) => CreateOrderWithItems(customerId, product);

  public static Validation<OrderValidationErrors, Order> CreateOrder(Guid customerId) => Order
      .CreateOrder(customerId, _orderSeedData.Street, _orderSeedData.City, _orderSeedData.Country, _orderSeedData.PostCode, _orderSeedData.CardHolderName, _orderSeedData.CardNumber, _orderSeedData.Expiration, _orderSeedData.PaymentMethod);

    public static Validation<OrderValidationErrors, Order> CreateOrderWithItems(Guid customerId, Product product) => Order
      .CreateOrder(customerId, _orderSeedData.Street, _orderSeedData.City, _orderSeedData.Country, _orderSeedData.PostCode, _orderSeedData.CardHolderName, _orderSeedData.CardNumber, _orderSeedData.Expiration, _orderSeedData.PaymentMethod)
      .Bind(order => order.AddOrderItem(OrderId.New().Value, product.ProductId.Value, product.Price.Value, product.Name.Value, product.Description.Value, product.Quantity.Value));
}
  public static class ProductSeedData
  {
    public record ProductSeed(string name, string description, decimal price, int quantity);

    private static readonly ProductSeed _productOneSeedData = new("Frying Pan", "Stainless Steel.", 19.99m, 1);
    private static readonly ProductSeed _productTwoSeedData = new("Apple Charger", "Charge your iphone with this state of the art charger.", 49.99m, 3);

    public static IEnumerable<Product> CreateProducts() => new[]
        {
          TryCreateProduct(_productOneSeedData),
          TryCreateProduct(_productTwoSeedData),
        }
          .Bind(validation => validation.Match(
              product => new[] { product },
              errors => Enumerable.Empty<Product>()
          ));

    public static Validation<ProductValidationErrors, Product> TryCreateProduct(ProductSeed _productSeedData) => CreateProduct(_productSeedData);

    public static Validation<ProductValidationErrors, Product> CreateProduct(ProductSeed _productSeedData) => Product
        .CreateProduct(_productSeedData.name, _productSeedData.description, _productSeedData.price, _productSeedData.quantity);
  }

  public static class CustomerSeedData
  {
    public record CustomerSeed(string name, string email);

    private static readonly CustomerSeed _customerOneSeedData = new("Christopher", "christopher@gmail.com");
    private static readonly CustomerSeed _customerTwoSeedData = new("Rebecca", "rebecca@gmail.com");

    public static IEnumerable<Customer> CreateCustomers() => new[]
        {
          TryCreateCustomer(_customerOneSeedData),
          TryCreateCustomer(_customerTwoSeedData),
        }
          .Bind(validation => validation.Match(
              customer => new[] { customer },
              errors => Enumerable.Empty<Customer>()
          ));

    public static Validation<CustomerValidationErrors, Customer> TryCreateCustomer(CustomerSeed customerSeedData) => CreateCustomer(customerSeedData);

    public static Validation<CustomerValidationErrors, Customer> CreateCustomer(CustomerSeed customerSeedData) => Customer
        .CreateCustomer(customerSeedData.name, customerSeedData.email);

  }
}
