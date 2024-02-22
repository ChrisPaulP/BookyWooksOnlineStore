using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace BookWooks.OrderApi.Web.Orders;

public class OrderFulfillmentRequest
{
  public const string Route = "/Orders/{Order:guid}/OrderFulfilled";
  public static string BuildRoute(Guid orderId) => Route.Replace("{OrderId:guid}", orderId.ToString());

  [Required]
  [FromRoute]
  public Guid OrderId { get; set; }
}
