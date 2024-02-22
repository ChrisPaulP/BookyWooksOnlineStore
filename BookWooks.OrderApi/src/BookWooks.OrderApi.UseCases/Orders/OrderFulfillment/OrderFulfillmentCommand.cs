using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookyWooks.SharedKernel;

namespace BookWooks.OrderApi.UseCases.Orders.OrderFulfillment;
public record OrderFulfillmentCommand(Guid OrderId) : ICommand<Result<Guid>>;

