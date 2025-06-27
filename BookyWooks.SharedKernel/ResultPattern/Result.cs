using BookyWooks.SharedKernel.Validation;

namespace BookyWooks.SharedKernel.ResultPattern;

//public class Result<T>
//{
//    public bool Success { get; }
//    public T? Data { get; }
//    public IEnumerable<Error> Errors { get; }

//    public Result(T data) => (Success, Data, Errors) = (true, data, new List<Error>());
//    public Result(IEnumerable<Error> errors) => (Success, Errors) = (false, errors);

//    //public new static StandardResult NotFound(params string[] errorMessages)
//    //{
//    //    return new StandardResult(ResultStatus.NotFound) { Errors = errorMessages };
//    //}


//}

//public enum ResultStatusEnum
//{
//    Ok,
//    Error,
//    Forbidden,
//    Unauthorized,
//    Invalid,
//    NotFound,
//    Conflict,
//    CriticalError,
//    Unavailable
//}
public class Result<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public IEnumerable<Error> Errors { get; }

    public Result(T data)
    {
        Success = true;
        Data = data;
        Errors = Enumerable.Empty<Error>();
    }

    public Result(IEnumerable<Error> errors)
    {
        Success = false;
        Data = default;
        Errors = errors;
    }

    public Result(params Error[] errors) : this(errors.AsEnumerable()) { }
}

public static class Result
{
    public static Result<T> Ok<T>(T data) => new Result<T>(data);
    public static Result<T> Fail<T>(params Error[] errors) => new Result<T>(errors);
}

public static class ResultExtensions
{
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
    {
        return result.Success
            ? new Result<TOut>(mapper(result.Data!))
            : new Result<TOut>(result.Errors);
    }

    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> binder)
    {
        return result.Success
            ? binder(result.Data!)
            : new Result<TOut>(result.Errors);
    }

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> binder)
    {
        return result.Success
            ? await binder(result.Data!)
            : new Result<TOut>(result.Errors);
    }
}