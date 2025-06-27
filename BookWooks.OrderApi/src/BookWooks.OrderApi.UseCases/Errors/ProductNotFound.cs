using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.UseCases.Errors;
public record ProductNotFound() : Error("Product not found")
{
  public static ProductNotFound Create() => new();
}

