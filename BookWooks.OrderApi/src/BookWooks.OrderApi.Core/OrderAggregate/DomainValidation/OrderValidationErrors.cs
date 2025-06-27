
namespace BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;

public record struct OrderErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct DeliveryAddressErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct PaymentErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct CustomerIdErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct OrderItemValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct OrderIdValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct ProductValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct CustomerValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);

[GenerateOneOf]
public partial class OrderValidationErrors : OneOfBase<OrderErrors, CustomerIdErrors, DeliveryAddressErrors, PaymentErrors, OrderItemValidationErrors, OrderIdValidationErrors>;



