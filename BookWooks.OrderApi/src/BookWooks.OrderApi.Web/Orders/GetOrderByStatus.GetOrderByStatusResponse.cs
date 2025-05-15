namespace BookWooks.OrderApi.Web.Orders;

public record GetOrderByStatusResponse(List<OrderWithItemsRecord> Orders);
