namespace BookyWooks.SharedKernel.Validation;
public record DomainValidationError(string errorMessage, HttpErrorType httpErrorType = HttpErrorType.Validation) : Error($"Validation error in field: {errorMessage}")
{
    public static DomainValidationError Create(string errorMessage) => new(errorMessage);
}
public enum ValidationSeverity
{
    Error,
    Warning,
    Info
}