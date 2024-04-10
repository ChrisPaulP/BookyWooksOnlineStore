namespace BookWooks.OrderApi.Infrastructure.Data.Queries;
public class FakeGetOrdersByStatusQueryService : IGetOrdersByStatusQueryService
{
  public async Task<IEnumerable<OrderDTO>> GetOrdersByStatusAsync(string status)
  {
    var testProject = new OrderDTO(Guid.NewGuid(), "Test Order Status", null);
    return await Task.FromResult(new List<OrderDTO> { testProject });
  }
}
