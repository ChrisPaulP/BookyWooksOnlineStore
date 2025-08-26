namespace BookWooks.OrderApi.UseCases.Orders.AiServices;
public interface ICustomerSupportService
{
 Task<string> CustomerSupportAsync(string query);
}
