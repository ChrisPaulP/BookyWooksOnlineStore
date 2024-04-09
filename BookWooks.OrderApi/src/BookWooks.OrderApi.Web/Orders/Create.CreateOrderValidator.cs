
namespace BookWooks.OrderApi.Web.Orders;

public class CreateOrderValidator : Validator<CreateOrderRequest>
{
  public CreateOrderValidator()
#pragma warning disable S125 // Sections of code should not be commented out
  {
    //RuleFor(x => x.Name)
    //  .NotEmpty()
    //  .WithMessage("Name is required.")
    //  .MinimumLength(2)
    //  .MaximumLength(DataSchemaConstants.DEFAULT_NAME_LENGTH);
  }
#pragma warning restore S125 // Sections of code should not be commented out
}
