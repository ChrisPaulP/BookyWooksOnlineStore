

namespace BookyWooks.SharedKernel;

public static class GuardChecks
{
    public static void NullOrWhiteSpace(this IGuardClause guardClause, string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} cannot be null, empty, or whitespace.", parameterName);
        }
    }

    public static void Null(this IGuardClause guardClause, object value, string parameterName)
    {
        if (value == null)
        {
            throw new ArgumentException($"{parameterName} cannot be null, empty, or whitespace.", parameterName);
        }
    }

    public static void NegativeNumbers(this IGuardClause guardClause, int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be a positive integer.");
        }
    }
}
