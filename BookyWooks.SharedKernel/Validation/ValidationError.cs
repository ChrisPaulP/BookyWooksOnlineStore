namespace BookyWooks.SharedKernel.Validation;
public record ValidationError(string errorMessage, HttpErrorType httpErrorType = HttpErrorType.Validation) : Error($"Validation error in field: {errorMessage}")
{
    public static ValidationError Create(string errorMessage) => new(errorMessage);
}
public enum ValidationSeverity
{
    Error,
    Warning,
    Info
}