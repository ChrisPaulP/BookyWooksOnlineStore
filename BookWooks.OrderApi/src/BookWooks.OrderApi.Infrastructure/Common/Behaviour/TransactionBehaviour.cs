
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//using Serilog.Context;

//using ILogger = Serilog.ILogger;

//using BookWooks.OrderApi.Infrastructure.Data;

//namespace BookWooks.OrderApi.Infrastructure.Common.Behaviour;
//public class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
//{
//  private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;
//  private readonly BookyWooksOrderDbContext _orderingDbContext;
//  private readonly IOrderIntegrationEventService _orderingIntegrationEventService;

//  public TransactionBehaviour(BookyWooksOrderDbContext orderingDbContext,
//      IOrderIntegrationEventService orderingIntegrationEventService,
//      ILogger<TransactionBehaviour<TRequest, TResponse>> logger)
//  {
//    _orderingDbContext = orderingDbContext ?? throw new ArgumentException(nameof(BookyWooksOrderDbContext));
//    _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentException(nameof(orderingIntegrationEventService));
//    _logger = logger ?? throw new ArgumentException(nameof(ILogger));
//  }

//  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//  {
//    var response = default(TResponse);

//    try
//    {
//      if (_orderingDbContext.HasActiveTransaction)
//      {
//        return await next();
//      }

//      var strategy = _orderingDbContext.Database.CreateExecutionStrategy();

//      await strategy.ExecuteAsync(async () =>
//      {
//        using var transaction = await _orderingDbContext.BeginTransactionAsync();

//        if (transaction == null)
//        {
//          _logger.LogError("Transaction is unexpectedly null");
//          return;
//        }

//        var transactionId = transaction.TransactionId;

//        using (LogContext.PushProperty("TransactionContext", transactionId))
//        {
//          _logger.LogInformation("----- Begin transaction {TransactionId} for ({@Command})", transactionId, request);

//          response = await next();

//          _logger.LogInformation("----- Commit transaction {TransactionId}", transactionId);

//          await _orderingDbContext.CommitTransactionAsync(transaction);
//        }

//        await _orderingIntegrationEventService.PublishEventsThroughEventBusAsync(transactionId);
//      });

//      return response!;
//    }
//    catch (Exception ex)
//    {
//      var detailedExceptionMessage = ex.ToString();
//      _logger.LogError(detailedExceptionMessage, "ERROR Handling transaction for {CommandName} ({@Command})", request);
//      throw;
//    }
//  }
//}

////Notes:
////In the provided TransactionBehaviour class, the handling of the request logic ends after the next() delegate is invoked.Let's break it down:

////response = await next();
////This line is where the next() delegate, which represents the next step in the request processing pipeline, is invoked.This delegate typically represents the actual request handling logic, which may involve executing business logic, querying a database, or performing other operations related to the request.

////Once the next() delegate is invoked, control passes to the next component in the request pipeline, and the processing of the request logic is considered complete from the perspective of TransactionBehaviour.Any further operations or actions that need to be performed after handling the request occur after this line.

////Therefore, in the context of TransactionBehaviour, the handling of the request logic ends after the await next() call, and subsequent actions such as committing the transaction and publishing integration events occur afterward.
