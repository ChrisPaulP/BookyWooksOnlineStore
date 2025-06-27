namespace BookWooks.OrderApi.Web.Products;

public class GetProductsRequest: IRequestWithRoute
{
  public static string Route => "/Products/{Prompt}";
  public static string BuildRoute(string prompt) => Route.Replace("{Prompt:string}", prompt);
  public string Prompt { get; set; } = string.Empty;
}

