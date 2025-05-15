using BookyWooks.SharedKernel.Validation;

namespace BookyWooks.SharedKernel.ResultPattern;

public class Result<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public IEnumerable<Error> Errors { get; }

    public Result(T data) => (Success, Data, Errors) = (true, data, new List<Error>());
    public Result(IEnumerable<Error> errors) => (Success, Errors) = (false, errors);

    //public new static StandardResult NotFound(params string[] errorMessages)
    //{
    //    return new StandardResult(ResultStatus.NotFound) { Errors = errorMessages };
    //}

    
}

public enum ResultStatusEnum
{
    Ok,
    Error,
    Forbidden,
    Unauthorized,
    Invalid,
    NotFound,
    Conflict,
    CriticalError,
    Unavailable
}
