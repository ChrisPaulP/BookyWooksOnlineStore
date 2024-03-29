using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

namespace BookWooks.OrderApi.Core.OrderAggregate.Entities;
public class Customer : EntityBase
{
  public string Name { get; private set; } = string.Empty;
  public string Email { get; private set; } = string.Empty;

  public static Customer Create(string name, string email)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentException.ThrowIfNullOrWhiteSpace(email);

    var customer = new Customer
    {
      Name = name,
      Email = email
    };
    return customer;
  }
}
