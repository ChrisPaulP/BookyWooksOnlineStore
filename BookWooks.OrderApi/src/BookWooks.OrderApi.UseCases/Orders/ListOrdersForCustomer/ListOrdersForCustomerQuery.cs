﻿namespace BookWooks.OrderApi.UseCases.Orders.ListOrdersForCustomer;
public record ListOrdersForCustomerQuery(int? Skip, int? Take, Guid CustomerId) : IQuery<DetailedResult<IEnumerable<OrderDTO>>>;
