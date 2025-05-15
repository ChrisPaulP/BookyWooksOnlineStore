namespace BookyWooks.SharedKernel.Validation;

public class CommandValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, DetailedResult<TResponse>> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public CommandValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<DetailedResult<TResponse>> Handle(TRequest request, RequestHandlerDelegate<DetailedResult<TResponse>> next, CancellationToken cancellationToken)
    {
        var errors = _validators
            .Select(v => v.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        if (errors.Any())
        {
            var errorBuilder = new StringBuilder();

            errorBuilder.AppendLine("Invalid command, reason: ");

            foreach (var error in errors)
            {
                errorBuilder.AppendLine(error.ErrorMessage);
            }

            return DetailedResult<TResponse>.Invalid(errors.Select(e => new ValidationError(e.ErrorMessage,HttpErrorType.Validation)).ToList());
        }
        return await next();
    }
}

//Example of CreateOrderValidator that can be used for CreadteOrderCommand:
//public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
//{
//    public CreateOrderValidator()
//    {
//        RuleFor(x => x.Name)
//            .NotEmpty()
//            .MaximumLength(BrandName.MaxLength);

//        RuleFor(x => x.Description)
//            .MaximumLength(BrandDescription.MaxLength);
//    }
//}

//This could then be registered in the DI container in the ConfigureServices method in the Program class by doing the following:

//         services.AddMediatR(configuration => configuration
//        .RegisterServicesFromAssembly(Application.AssemblyReference.Assembly)
//        .AddOpenBehavior(typeof(ValidationPipelineBehavior<,>))
//        );


//        services.AddValidatorsFromAssembly(
//            Application.AssemblyReference.Assembly,
//            includeInternalTypes: true);