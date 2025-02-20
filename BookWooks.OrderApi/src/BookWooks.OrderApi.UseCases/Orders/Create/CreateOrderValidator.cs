using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace BookWooks.OrderApi.UseCases.Orders.Create;
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
  public CreateOrderValidator()
  {
    RuleFor(x => x.Name)
        .NotEmpty()
        .MaximumLength(BrandName.MaxLength);

    RuleFor(x => x.Description)
        .MaximumLength(BrandDescription.MaxLength);
  }
}
