

using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.UseCases.Create;
public record CreateOrderCommand(Guid Id, IEnumerable<OrderCommandOrderItem> OrderItems, string UserId, string UserName, string City, string Street, string Country, string Postcode,
        string CardNumber, string CardHolderName, DateTime ExpiryDate, string CardSecurityNumber) : ICommand<Result<Guid>>;
public record OrderCommandOrderItem(int BookId, string BookTitle, decimal BookPrice,  int Quantity, string BookImageUrl);

