using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.UseCases.Errors;
public class DatabaseErrors
{
  public record DatabaseError() : Error("A database error occurred");
  public record UnexpectedError() : Error("An unexpected error occurred");
  public record UnhandledError() : Error("Unhandled error");
  public record TimeoutError() : Error("A timeout error occurred");


}
