namespace BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;

public record struct CustomerValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);
