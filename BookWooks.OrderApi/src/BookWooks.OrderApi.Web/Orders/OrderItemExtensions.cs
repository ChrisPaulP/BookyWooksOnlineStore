namespace BookWooks.OrderApi.UseCases.Orders;
public static class OrderItemExtensions
{
  public static IEnumerable<CreateOrderItemCommand> ToOrderCommandOrderItems(this List<OrderRequestOrderItem> orderRequestOrderItems) =>
          orderRequestOrderItems.Select(item => item.ToOrderCommandOrderItem());
  private static CreateOrderItemCommand ToOrderCommandOrderItem(this OrderRequestOrderItem item)
  => new CreateOrderItemCommand(item.ProductId, item.ProductName, item.ProductDescription, item.Price, item.Quantity);
  
  public static IEnumerable<OrderItemRecord> ToOrderItemRecord(this IEnumerable<OrderItemDTO> items) =>
    items.Select(i => new OrderItemRecord(i.ProductId, i.ProductName, i.ProductDescription, i.Price, i.Quantity));
}

//In the code above, using yield in the ToOrderCommandOrderItems method offers several benefits:

//Lazy Evaluation: When you use yield, the ToOrderCommandOrderItems method doesn't generate all the ToOrderCommandOrderItems objects at once and store them in memory. Instead, it generates them one at a time as they are requested by the consumer of the iterator. This is especially useful when dealing with large collections of data because it avoids the need to load the entire collection into memory at once.

//Reduced Memory Usage: Since items are generated on-the-fly, you don't need to keep all of them in memory simultaneously. This can significantly reduce memory consumption, which is especially important when working with large collections.

//Improved Performance: yield can lead to better performance in certain scenarios, especially when you don't need to process the entire collection immediately. It allows you to start consuming items as they are generated, rather than waiting for the entire collection to be processed.

//Simplified Code: It often results in cleaner and more maintainable code because you don't need to manage the state of the iteration manually. The yield keyword takes care of this for you.

//Flexibility: Using yield makes it easier to implement custom filtering, mapping, or transformation logic within the iterator.You can customize the behavior of your iterator without altering the entire collection.

//Integration with LINQ: Lazy evaluation with yield integrates well with LINQ. You can use LINQ methods like Where, Select, and OrderBy on the iterator, and the operations are performed as items are generated, allowing for efficient and flexible data processing.

//In summary, using yield here allows you to create a generator for OrderItemDTO objects from a collection of BasketItem objects efficiently, with reduced memory usage and improved performance, especially when working with large datasets or when you don't need to process the entire collection immediately.
