namespace BookWooks.OrderApi.Core.OrderAggregate;
public class OrderItem : EntityBase<Guid>
{
  public decimal BookPrice { get; private set; }
  public string BookTitle { get; private set; }
  public string Message { get; private set; }
  public string BookImageUrl { get; private set; }
  public int Quantity { get; private set; }
  public int BookId { get; private set; }
  protected OrderItem(string bookTtile, string bookImageUrl) 
  { 
    Message = "New Order Item Added";
    BookTitle = Guard.Against.NullOrEmpty(bookTtile, nameof(bookTtile));
    BookImageUrl = Guard.Against.NullOrEmpty(bookImageUrl, nameof(bookImageUrl));
  }

  public OrderItem(int bookId, string bookTitle, decimal bookPrice, string bookImageUrl, int quantity = 1) : this(bookTitle, bookImageUrl)
  {
  
    BookId = bookId;
    BookPrice = bookPrice;
    Quantity = quantity;
  }
  public void AddToQuantity(int quantity)
  {
    if (quantity < 0)
    {
      //throw new GenericOrderingDomainException("Invalid units");
    }

    Quantity += quantity;
  }
  public void RemoveFromQuantity(int quantity)
  {
    if (quantity < 0)
    {
     // throw new GenericOrderingDomainException("There are no Items to remove");
    }

    Quantity -= quantity;
  }
}
