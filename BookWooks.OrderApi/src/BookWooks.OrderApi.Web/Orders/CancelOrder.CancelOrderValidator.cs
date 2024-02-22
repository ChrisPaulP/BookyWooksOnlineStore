using FastEndpoints;

namespace BookWooks.OrderApi.Web.Orders;

public class CancelOrderValidator : Validator<CancelOrderRequest>
{
  public CancelOrderValidator()
  {
    //RuleFor(x => x.Name)
    //  .NotEmpty()
    //  .WithMessage("Name is required.")
    //  .MinimumLength(2)
    //  .MaximumLength(DataSchemaConstants.DEFAULT_NAME_LENGTH);
  }
}

