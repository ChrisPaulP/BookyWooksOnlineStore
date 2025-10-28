
namespace BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;

public record struct OrderValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct DeliveryAddressValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct PaymentValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct CustomerIdValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct OrderItemValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct OrderIdValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct ProductValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct CustomerValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct OrderStatusValidationError(string Field, BusinessRuleError Error);

[GenerateOneOf]
public partial class OrderDomainValidationErrors : OneOfBase<OrderValidationErrors, CustomerIdValidationErrors, DeliveryAddressValidationErrors, PaymentValidationErrors, OrderItemValidationErrors, OrderIdValidationErrors, OrderStatusValidationError>;



