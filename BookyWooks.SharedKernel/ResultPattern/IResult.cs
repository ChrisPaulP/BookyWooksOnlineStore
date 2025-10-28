using BookyWooks.SharedKernel.Validation;

namespace BookyWooks.SharedKernel.ResultPattern;
public interface IResult
{
    ResultStatus Status { get; }

    IEnumerable<string> Errors { get; }

    List<DomainValidationError> DomainValidationErrors { get; }

    Type ValueType { get; }

    object GetValue();
}
