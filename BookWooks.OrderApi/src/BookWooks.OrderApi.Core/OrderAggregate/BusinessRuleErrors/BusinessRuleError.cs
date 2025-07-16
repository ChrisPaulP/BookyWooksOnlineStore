using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Core.OrderAggregate.BusinessRuleErrors;
public record BusinessRuleError(string errorMessage) : Error(errorMessage)
{
  public static BusinessRuleError Create(string errorMessage) => new(errorMessage);
  public override string ToString() => errorMessage;
}
