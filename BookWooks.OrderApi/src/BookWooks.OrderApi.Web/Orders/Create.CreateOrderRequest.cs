using System;
using System.ComponentModel.DataAnnotations;


namespace BookWooks.OrderApi.Web.Orders;

public class CreateOrderRequest
{
  public const string Route = "/Orders";

  [Required]
  public Guid Id { get; set; }

  [Required]
  public Guid CustomerId { get; set; }

  [Required]
  public OrderRequestAddress Address { get; set; } = new();

  [Required]
  public OrderRequestPayment Payment { get; set; } = new();

  [Required]
  public List<OrderRequestOrderItem> OrderItems { get; set; } = new();
}

public record OrderRequestOrderItem
{
  [Required]
  public decimal Price { get; init; }

  [Required]
  public int Quantity { get; init; } = 1;

  [Required]
  public Guid ProductId { get; init; }

}

public record OrderRequestAddress
{
  [Required]
  public string Street { get; init; } = string.Empty;

  [Required]
  public string City { get; init; } = string.Empty;

  [Required]
  public string Country { get; init; } = string.Empty;

  [Required]
  public string Postcode { get; init; } = string.Empty;
}

public record OrderRequestPayment
{
  [Required]
  public string CardHolderName { get; init; } = string.Empty;

  [Required]
  public string CardNumber { get; init; } = string.Empty;

  [Required]
  public string ExpiryDate { get; init; } = string.Empty;

  [Required]
  public string Cvv { get; init; } = string.Empty;

  [Required]
  public int PaymentMethod { get; init; }
}

