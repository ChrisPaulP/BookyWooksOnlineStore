using BookWooks.OrderApi.Core.OrderAggregate.BusinessRuleErrors;
using BookyWooks.SharedKernel.Validation;

namespace BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;

public record struct DeliveryAddressErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct PaymentErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct CustomerIdErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
public record struct OrderItemValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);

[GenerateOneOf]
public partial class OrderValidationErrors : OneOfBase<CustomerIdErrors, DeliveryAddressErrors, PaymentErrors, OrderItemValidationErrors>;



