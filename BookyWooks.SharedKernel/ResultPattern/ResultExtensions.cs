using BookyWooks.SharedKernel.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using static MassTransit.ValidationResultExtensions;


namespace BookyWooks.SharedKernel.ResultPattern;

public static class ResultExtensions2
{

    public static TResult Match<T, TResult>(
        this DetailedResult<T> result,
        Func<T, TResult> onSuccess,
        Func<IEnumerable<ValidationError>, TResult> onValidationFailure,
        Func<IEnumerable<string>, TResult> onDomainFailure)
       // where TResult : IResponse
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onDomainFailure == null) throw new ArgumentNullException(nameof(onDomainFailure));
        if (onValidationFailure == null) throw new ArgumentNullException(nameof(onValidationFailure));

        return result switch
        {
            { IsSuccess: true } => onSuccess(result.Value),
            { ValidationErrors: { Count: > 0 } } => onValidationFailure(result.ValidationErrors),
            _ => onDomainFailure(result.Errors)
        };
    }

    public static T MatchTwo<T>(
        this StandardResult result,
        Func<T> onSuccess,
        Func<IEnumerable<string>, T> onFailure)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));

        return result.IsSuccess ? onSuccess() : onFailure(result.Errors);
    }

    /// <summary>
    /// Transforms a Result's type from a source type to a destination type. If the Result is successful, the func parameter is invoked on the Result's source value to map it to a destination type.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="result"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception> 
    public static DetailedResult<TDestination> Map<TSource, TDestination>(this DetailedResult<TSource> result, Func<TSource, TDestination> func)
    {
        switch (result.Status)
        {
            case ResultStatus.Ok: return func(result);
            case ResultStatus.NotFound:
                return result.Errors.Any()
                    ? DetailedResult<TDestination>.NotFound(result.Errors.ToArray())
                    : DetailedResult<TDestination>.NotFound();
            case ResultStatus.Unauthorized: return DetailedResult<TDestination>.Unauthorized();
            case ResultStatus.Forbidden: return DetailedResult<TDestination>.Forbidden();
            case ResultStatus.Invalid: return DetailedResult<TDestination>.Invalid(result.ValidationErrors);
            case ResultStatus.Error: return DetailedResult<TDestination>.Error(result.Errors.ToArray());
            case ResultStatus.Conflict:
                return result.Errors.Any()
                                    ? DetailedResult<TDestination>.Conflict(result.Errors.ToArray())
                                    : DetailedResult<TDestination>.Conflict();
            case ResultStatus.CriticalError: return DetailedResult<TDestination>.CriticalError(result.Errors.ToArray());
            case ResultStatus.Unavailable: return DetailedResult<TDestination>.Unavailable(result.Errors.ToArray());
            default:
                throw new NotSupportedException($"Result {result.Status} conversion is not supported.");
        }
    }
}
