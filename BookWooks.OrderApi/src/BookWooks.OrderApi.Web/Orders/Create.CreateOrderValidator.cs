using FastEndpoints;

namespace BookWooks.OrderApi.Web.Orders;

public class CreateOrderValidator : Validator<CreateOrderRequest>
{
  public CreateOrderValidator()
  {
    //RuleFor(x => x.Name)
    //  .NotEmpty()
    //  .WithMessage("Name is required.")
    //  .MinimumLength(2)
    //  .MaximumLength(DataSchemaConstants.DEFAULT_NAME_LENGTH);
  }
}
