using System.ComponentModel.DataAnnotations;

namespace BookWooks.OrderApi.Web.Orders;

public class CancelOrderRequest
{
    public const string Route = "/Orders/Cancel/{OrderId:guid}";
    public static string BuildRoute(Guid orderId) => Route.Replace("{OrderId:guid}", orderId.ToString());
    [Required]
    public Guid OrderId { get; set; }
}
