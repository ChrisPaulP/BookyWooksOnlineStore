namespace BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;

public record struct ProductValidationErrors(string Field, IReadOnlyList<BusinessRuleError> Errors);



