using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOf;

namespace BookWooks.OrderApi.UseCases.Errors;
[GenerateOneOf]
public partial class CreateOrderErrors : OneOfBase<ValidationErrors, NetworkErrors>, IError;
