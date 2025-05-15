namespace BookWooks.OrderApi.Web.Interfaces;

public interface IRequestWithRoute
{
  static abstract string Route { get; }
}
