namespace BookWooks.OrderApi.UseCases.FunctionalExtensions;

public static class EitherExtensions
{
    /// <summary>
    /// Converts a nullable entity to an Either type
    /// </summary>
    public static Either<TLeft, TRight> ToEither<TLeft, TRight>(
        this TRight? entity,
        Func<TLeft> leftValueFactory)
        where TRight : class
    {
        return entity != null
            ? Right(entity)
            : Left(leftValueFactory());
    }

    /// <summary>
    /// Converts an enumerable to an Either type, returning Left if empty
    /// </summary>
    public static Either<TLeft, IEnumerable<TRight>> ToEither<TLeft, TRight>(
        this IEnumerable<TRight> source,
        Func<TLeft> leftValueFactory)
    {
        return source.Any()
            ? Right<TLeft, IEnumerable<TRight>>(source)
            : Left<TLeft, IEnumerable<TRight>>(leftValueFactory());
    }

    /// <summary>
    /// Converts an order status validation to an Either type
    /// </summary>
    public static Either<OrderErrors, T> ToEither<T>(
        this Validation<OrderStatusValidationError, T> validation,
        Func<OrderStatusValidationError, OrderErrors> mapDomainError)
    {
        return validation.Match(
            success => Either<OrderErrors, T>.Right(success),
            error => Either<OrderErrors, T>.Left(mapDomainError(error.First()))
        );
    }
}
