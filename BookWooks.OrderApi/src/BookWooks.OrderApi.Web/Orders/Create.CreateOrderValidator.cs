namespace BookWooks.OrderApi.Web.Orders;

  public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
  {
    public CreateOrderRequestValidator()
    {
      RuleFor(x => x.Id).NotEmpty();
      RuleFor(x => x.CustomerId).NotEmpty();
      RuleFor(x => x.Address).SetValidator(new OrderRequestAddressValidator());
      RuleFor(x => x.Payment).SetValidator(new OrderRequestPaymentValidator());
      RuleForEach(x => x.OrderItems).SetValidator(new OrderRequestOrderItemValidator());
    }
  }

  public class OrderRequestOrderItemValidator : AbstractValidator<OrderRequestOrderItem>
  {
    public OrderRequestOrderItemValidator()
    {
      RuleFor(x => x.Price).GreaterThan(0);
      RuleFor(x => x.Quantity).GreaterThan(0);
      RuleFor(x => x.ProductId).NotEmpty();
    }
  }

  public class OrderRequestAddressValidator : AbstractValidator<OrderRequestAddress>
  {
    public OrderRequestAddressValidator()
    {
      RuleFor(x => x.Street).NotEmpty().MaximumLength(100);
      RuleFor(x => x.City).NotEmpty().MaximumLength(50);
      RuleFor(x => x.Country).NotEmpty().MaximumLength(50);
      RuleFor(x => x.Postcode).NotEmpty().MaximumLength(20);
    }
  }

  public class OrderRequestPaymentValidator : AbstractValidator<OrderRequestPayment>
  {
    public OrderRequestPaymentValidator()
    {
      RuleFor(x => x.CardHolderName).NotEmpty().MaximumLength(100);
      RuleFor(x => x.CardHolderNumber).CreditCard();
      RuleFor(x => x.ExpiryDate).Matches(@"^(0[1-9]|1[0-2])\/?([0-9]{4}|[0-9]{2})$");
      RuleFor(x => x.Cvv).NotEmpty().Length(3, 4);
      RuleFor(x => x.PaymentMethod).IsInEnum();
    }
  }
