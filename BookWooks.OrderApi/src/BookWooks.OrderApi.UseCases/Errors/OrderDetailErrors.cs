using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOf;
using static BookWooks.OrderApi.UseCases.Errors.DatabaseErrors;

namespace BookWooks.OrderApi.UseCases.Errors;

[GenerateOneOf]
public partial class OrderErrors: OneOfBase<OrderNotFound, NetworkErrors>, IError;

[GenerateOneOf]
public partial class NetworkErrors : OneOfBase<TimeoutError, UnexpectedError, DatabaseError, UnhandledError>, IError;


