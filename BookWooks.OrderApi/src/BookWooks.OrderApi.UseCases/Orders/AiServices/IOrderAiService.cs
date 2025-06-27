using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.UseCases.Orders.AiServices;
public interface IOrderAiService<T>// where T : class, new()
{
  public Task<string> CustomerSupportAsync(string query);
  public Task<IEnumerable<T>> SearchProductsAsync(string query);
}
