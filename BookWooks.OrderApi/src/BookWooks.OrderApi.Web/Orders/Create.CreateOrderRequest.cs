using System;
using System.ComponentModel.DataAnnotations;


namespace BookWooks.OrderApi.Web.Orders;

public class CreateOrderRequest
{
  public const string Route = "/Orders";

  [Required]
  public Guid Id { get; set; }
  [Required]
  public string? Name { get; set; }
  [Required]
  public List<OrderRequestOrderItem> OrderItems { get; set; } = new List<OrderRequestOrderItem>();
  [Required]
  public string? UserId { get; set; }
  [Required]
  public string? UserName { get; set; }
  [Required]
  public string? City { get; set; }
  [Required]
  public string? Street { get; set; }
  [Required]
  public string? Country { get; set; }
  [Required]
  public string? Postcode { get; set; }
  [Required]
  public string? CardNumber { get; set; }
  [Required]
  public string? CardHolderName { get; set; }
  [Required]
  public DateTime ExpiryDate { get; set; }
  [Required]
  public string? CardSecurityNumber { get; set; }

}

public class OrderRequestOrderItem
{
  [Required]
  public decimal BookPrice { get; set; }
  [Required]
  public string? BookTitle { get; set; }
  [Required]
  public int Quantity { get; set; }
  [Required]
  public int BookId { get; set; }
  [Required]
  public string? BookImageUrl { get; set; }
}

//These properties need to be declared as nullable even when they're not expected to be null because when working with JSON serialization/deserialization libraries like Newtonsoft.Json (Json.NET) or the System.Text.Json library in .NET.
//These libraries often expect properties to be nullable for certain scenarios, such as deserializing JSON objects where a property might be absent.
