using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;
public class Product : EntityBase
{
  public Product(Guid id) : base(id) { }
  public string Title { get; private set; } = string.Empty;
  public string ImageUrl { get; private set; } = string.Empty;
  public decimal Price { get; private set; }
  
  public static Product Create(Guid Id, string title, string imageUrl, decimal price)
  {

    ArgumentException.ThrowIfNullOrEmpty(title);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
    var product = new Product(Id)
    {
      Title = title,
      ImageUrl = imageUrl,
      Price = price
    };

    return product;
  }
}
