namespace BookWooks.OrderApi.Web.Orders;

public static class CreateOrderCommandBuilder
{
  public static CreateOrderCommand Build(CreateOrderRequest request)
  {
    var address = new CreateAddressCommand(request.Address.Street,request.Address.City,request.Address.Country,request.Address.Postcode);
    var paymentDetails = new CreatePaymentDetailsCommand(request.Payment.CardHolderNumber,request.Payment.CardHolderName,request.Payment.ExpiryDate,request.Payment.PaymentMethod);
    var orderItems = request.OrderItems.Select(request => new CreateOrderItemCommand(request.ProductId,request.ProductName,request.ProductDescription,request.Price,request.Quantity)).ToList();

    return new CreateOrderCommand(request.CustomerId,address,paymentDetails,orderItems);
  }
}
