namespace BookWooks.OrderApi.UseCases.FunctionalExtensions;

public static class ValidationExtensions
{
    public static DomainValidationErrors MapValidationErrors(Seq<OrderDomainValidationErrors> failures)
    {
        return failures.MapValidationErrorDetails(
            GetErrorType,
            MapErrorMessages);
    }

    private static ErrorType GetErrorType(OrderDomainValidationErrors failure) =>
        failure.Match(
            orderErrors => ErrorType.Order,
            customerErrors => ErrorType.Customer,
            deliveryAddressErrors => ErrorType.DeliveryAddress,
            paymentErrors => ErrorType.Payment,
            orderItemErrors => ErrorType.OrderItem,
            orderIdErrors => ErrorType.OrderId,
            orderStatusValidationError => ErrorType.OrderStatusValidationError);

    private static IEnumerable<DomainValidationError> MapErrorMessages(OrderDomainValidationErrors failure) =>
        failure.Match(
            orderErrors => orderErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
            customerErrors => customerErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
            deliveryAddressErrors => deliveryAddressErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
            paymentErrors => paymentErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
            orderItemErrors => orderItemErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
            orderIdErrors => orderIdErrors.Errors.Select(e => DomainValidationError.Create(e.errorMessage)),
            orderStatusValidationError => new[] { DomainValidationError.Create(orderStatusValidationError.Error.errorMessage) });

    private static DomainValidationErrors MapValidationErrorDetails<T>(
        this Seq<T> failures,
        Func<T, ErrorType> getErrorType,
        Func<T, IEnumerable<DomainValidationError>> getErrors)
    {
        var errorsByType = failures
            .GroupBy(getErrorType)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(getErrors).ToList() as IReadOnlyList<DomainValidationError>);

        return new DomainValidationErrors(errorsByType);
    }
}
