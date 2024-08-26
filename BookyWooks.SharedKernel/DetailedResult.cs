namespace BookyWooks.SharedKernel;
public class DetailedResult<T> : IResult
{
    protected DetailedResult() { }

    public DetailedResult(T value)
    {
        Value = value;
    }

    protected internal DetailedResult(T value, string successMessage) : this(value)
    {
        SuccessMessage = successMessage;
    }

    protected DetailedResult(ResultStatus status)
    {
        Status = status;
    }

    public static implicit operator T(DetailedResult<T> result) => result.Value;
    public static implicit operator DetailedResult<T>(T value) => new DetailedResult<T>(value);

    public static implicit operator DetailedResult<T>(StandardResult result) => new DetailedResult<T>(default(T))
    {
        Status = result.Status,
        Errors = result.Errors,
        SuccessMessage = result.SuccessMessage,
        CorrelationId = result.CorrelationId,
        ValidationErrors = result.ValidationErrors,
    };

    public T Value { get; }

    [JsonIgnore]
    public Type ValueType => typeof(T);
    public ResultStatus Status { get; protected set; } = ResultStatus.Ok;
    public bool IsSuccess => Status == ResultStatus.Ok;
    public string SuccessMessage { get; protected set; } = string.Empty;
    public string CorrelationId { get; protected set; } = string.Empty;
    public IEnumerable<string> Errors { get; protected set; } = new List<string>();
    public List<ValidationError> ValidationErrors { get; protected set; } = new List<ValidationError>();

    public object GetValue()
    {
        return this.Value;
    }
    public PagedResult<T> ToPagedResult(PagedInfo pagedInfo)
    {
        var pagedResult = new PagedResult<T>(pagedInfo, Value)
        {
            Status = Status,
            SuccessMessage = SuccessMessage,
            CorrelationId = CorrelationId,
            Errors = Errors,
            ValidationErrors = ValidationErrors
        };

        return pagedResult;
    }

    public static DetailedResult<T> Success(T value)
    {
        return new DetailedResult<T>(value);
    }

    public static DetailedResult<T> Success(T value, string successMessage)
    {
        return new DetailedResult<T>(value, successMessage);
    }
    public static DetailedResult<T> Error(params string[] errorMessages)
    {
        return new DetailedResult<T>(ResultStatus.Error) { Errors = errorMessages };
    }

    public static DetailedResult<T> Invalid(ValidationError validationError)
    {
        return new DetailedResult<T>(ResultStatus.Invalid) { ValidationErrors = { validationError } };
    }

    public static DetailedResult<T> Invalid(params ValidationError[] validationErrors)
    {
        return new DetailedResult<T>(ResultStatus.Invalid) { ValidationErrors = new List<ValidationError>(validationErrors) };
    }

    public static DetailedResult<T> Invalid(List<ValidationError> validationErrors)
    {
        return new DetailedResult<T>(ResultStatus.Invalid) { ValidationErrors = validationErrors };
    }
    public static DetailedResult<T> NotFound()
    {
        return new DetailedResult<T>(ResultStatus.NotFound);
    }

    public static DetailedResult<T> NotFound(params string[] errorMessages)
    {
        return new DetailedResult<T>(ResultStatus.NotFound) { Errors = errorMessages };
    }

    public static DetailedResult<T> Forbidden()
    {
        return new DetailedResult<T>(ResultStatus.Forbidden);
    }
    public static DetailedResult<T> Unauthorized()
    {
        return new DetailedResult<T>(ResultStatus.Unauthorized);
    }

    public static DetailedResult<T> Conflict()
    {
        return new DetailedResult<T>(ResultStatus.Conflict);
    }

    public static DetailedResult<T> Conflict(params string[] errorMessages)
    {
        return new DetailedResult<T>(ResultStatus.Conflict) { Errors = errorMessages };
    }

    public static DetailedResult<T> CriticalError(params string[] errorMessages)
    {
        return new DetailedResult<T>(ResultStatus.CriticalError) { Errors = errorMessages };
    }

    public static DetailedResult<T> Unavailable(params string[] errorMessages)
    {
        return new DetailedResult<T>(ResultStatus.Unavailable) { Errors = errorMessages };
    }
}
public enum ResultStatus
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
public class PagedResult<T> : DetailedResult<T>
{
    public PagedResult(PagedInfo pagedInfo, T value) : base(value)
    {
        PagedInfo = pagedInfo;
    }
    public PagedInfo PagedInfo { get; }
}
public class PagedInfo
{

    public PagedInfo(long pageNumber, long pageSize, long totalPages, long totalRecords)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = totalPages;
        TotalRecords = totalRecords;
    }

    public long PageNumber { get; private set; }
    public long PageSize { get; private set; }
    public long TotalPages { get; private set; }
    public long TotalRecords { get; private set; }

    public PagedInfo SetPageNumber(long pageNumber)
    {
        PageNumber = pageNumber;

        return this;
    }

    public PagedInfo SetPageSize(long pageSize)
    {
        PageSize = pageSize;

        return this;
    }

    public PagedInfo SetTotalPages(long totalPages)
    {
        TotalPages = totalPages;

        return this;
    }

    public PagedInfo SetTotalRecords(long totalRecords)
    {
        TotalRecords = totalRecords;

        return this;
    }
}
public static class ResultExtensions
{
    /// <summary>
    /// Transforms a Result's type from a source type to a destination type. If the Result is successful, the func parameter is invoked on the Result's source value to map it to a destination type.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="result"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception> 
    public static DetailedResult<TDestination> Map<TSource, TDestination>(this DetailedResult<TSource> result, Func<TSource, TDestination> func)
    {
        switch (result.Status)
        {
            case ResultStatus.Ok: return func(result);
            case ResultStatus.NotFound:
                return result.Errors.Any()
                    ? DetailedResult<TDestination>.NotFound(result.Errors.ToArray())
                    : DetailedResult<TDestination>.NotFound();
            case ResultStatus.Unauthorized: return DetailedResult<TDestination>.Unauthorized();
            case ResultStatus.Forbidden: return DetailedResult<TDestination>.Forbidden();
            case ResultStatus.Invalid: return DetailedResult<TDestination>.Invalid(result.ValidationErrors);
            case ResultStatus.Error: return DetailedResult<TDestination>.Error(result.Errors.ToArray());
            case ResultStatus.Conflict:
                return result.Errors.Any()
                                    ? DetailedResult<TDestination>.Conflict(result.Errors.ToArray())
                                    : DetailedResult<TDestination>.Conflict();
            case ResultStatus.CriticalError: return DetailedResult<TDestination>.CriticalError(result.Errors.ToArray());
            case ResultStatus.Unavailable: return DetailedResult<TDestination>.Unavailable(result.Errors.ToArray());
            default:
                throw new NotSupportedException($"Result {result.Status} conversion is not supported.");
        }
    }
}