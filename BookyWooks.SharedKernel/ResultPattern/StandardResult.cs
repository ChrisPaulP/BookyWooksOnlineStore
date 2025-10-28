using BookyWooks.SharedKernel.Validation;

namespace BookyWooks.SharedKernel.ResultPattern;
public class StandardResult : DetailedResult<StandardResult>
{
    public StandardResult() { }

    protected internal StandardResult(ResultStatus status) : base(status) { }

    public static StandardResult Success()
    {
        return new StandardResult();
    }
    public static StandardResult SuccessWithMessage(string successMessage)
    {
        return new StandardResult() { SuccessMessage = successMessage };
    }

    public static DetailedResult<T> Success<T>(T value)
    {
        return new DetailedResult<T>(value);
    }
    public static DetailedResult<T> Success<T>(T value, string successMessage)
    {
        return new DetailedResult<T>(value, successMessage);
    }

    public new static StandardResult Error(params string[] errorMessages)
    {
        return new StandardResult(ResultStatus.Error) { Errors = errorMessages };
    }
    public static StandardResult ErrorWithCorrelationId(string correlationId, params string[] errorMessages)
    {
        return new StandardResult(ResultStatus.Error)
        {
            CorrelationId = correlationId,
            Errors = errorMessages
        };
    }

    public new static StandardResult Invalid(DomainValidationError validationError)
    {
        return new StandardResult(ResultStatus.Invalid) { DomainValidationErrors = { validationError } };
    }

    public new static StandardResult Invalid(params DomainValidationError[] validationErrors)
    {
        return new StandardResult(ResultStatus.Invalid) { DomainValidationErrors = new List<DomainValidationError>(validationErrors) };
    }

    public new static StandardResult Invalid(List<DomainValidationError> validationErrors)
    {
        return new StandardResult(ResultStatus.Invalid) { DomainValidationErrors = validationErrors };
    }

    public new static StandardResult NotFound()
    {
        return new StandardResult(ResultStatus.NotFound);
    }

    public new static StandardResult NotFound(params string[] errorMessages)
    {
        return new StandardResult(ResultStatus.NotFound) { Errors = errorMessages };
    }
    public new static StandardResult Forbidden()
    {
        return new StandardResult(ResultStatus.Forbidden);
    }
    public new static StandardResult Unauthorized()
    {
        return new StandardResult(ResultStatus.Unauthorized);
    }

    public new static StandardResult Conflict()
    {
        return new StandardResult(ResultStatus.Conflict);
    }

    public new static StandardResult Conflict(params string[] errorMessages)
    {
        return new StandardResult(ResultStatus.Conflict) { Errors = errorMessages };
    }
    public new static StandardResult Unavailable(params string[] errorMessages)
    {
        return new StandardResult(ResultStatus.Unavailable) { Errors = errorMessages };
    }

    public static StandardResult CriticalError(params string[] errorMessages)
    {
        return new StandardResult(ResultStatus.CriticalError) { Errors = errorMessages };
    }
}
