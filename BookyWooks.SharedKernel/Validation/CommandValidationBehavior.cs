using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookyWooks.SharedKernel.ResultPattern;
using FluentValidation;

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

            return DetailedResult<TResponse>.Invalid(errors.Select(e => new ValidationError
            {
                ErrorMessage = e.ErrorMessage,
                ErrorCode = e.ErrorCode,
                Severity = ValidationSeverity.Error
            }).ToList());
        }

        return await next();
    }
}
