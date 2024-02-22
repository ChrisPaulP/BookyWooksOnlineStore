using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutBoxPattern;
    public class ResilientTransaction
    {
        private DbContext _context;
        private ResilientTransaction(DbContext context) =>
            _context = context ?? throw new ArgumentNullException(nameof(context));

        public static ResilientTransaction New(DbContext context) => new(context);

        public async Task ExecuteAsync(Func<Task> action)
        {
            //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
            //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    await action();
                    transaction.Commit();
                }
            });
        }
    }


//Execution strategies and explicit transactions using BeginTransaction and multiple DbContexts

//When retries are enabled in EF Core connections, each operation you perform using EF Core becomes its own retryable operation.
//Each query and each call to SaveChanges will be retried as a unit if a transient failure occurs.

//However, if your code initiates a transaction using BeginTransaction, you're defining your own group of operations that need to be treated as a unit.
//Everything inside the transaction has to be rolled back if a failure occurs.

//If you try to execute that transaction when using an EF execution strategy(retry policy) and you call SaveChanges from multiple DbContexts,
//you'll get an exception like this one:

//System.InvalidOperationException: The configured execution strategy 'SqlServerRetryingExecutionStrategy' does not support user initiated transactions.
//Use the execution strategy returned by 'DbContext.Database.CreateExecutionStrategy()' to execute all the operations in the transaction as a retriable unit.

//KEYPOINT 1 -- The solution is to manually invoke the EF execution strategy with a delegate representing everything that needs to be executed.If a transient failure occurs,
//the execution strategy will invoke the delegate again.For example, the following code shows how it's implemented in eShopOnContainers with two multiple DbContexts
//(_catalogContext and the IntegrationEventLogContext) when updating a product and then saving the ProductPriceChangedIntegrationEvent object, which needs to use a different DbContext.

//public async Task<IActionResult> UpdateProduct(
//    [FromBody] CatalogItem productToUpdate)
//{
//    // Other code ...

//    var oldPrice = catalogItem.Price;
//    var raiseProductPriceChangedEvent = oldPrice != productToUpdate.Price;

//    // Update current product
//    catalogItem = productToUpdate;

//    // Save product's data and publish integration event through the Event Bus
//    // if price has changed
//    if (raiseProductPriceChangedEvent)
//    {
//        //Create Integration Event to be published through the Event Bus
//        var priceChangedEvent = new ProductPriceChangedIntegrationEvent(
//          catalogItem.Id, productToUpdate.Price, oldPrice);

//        // Achieving atomicity between original Catalog database operation and the
//        // IntegrationEventLog thanks to a local transaction
//        await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(
//            priceChangedEvent);

//        // Publish through the Event Bus and mark the saved event as published
//        await _catalogIntegrationEventService.PublishThroughEventBusAsync(
//            priceChangedEvent);
//    }
//    // Just save the updated product because the Product's Price hasn't changed.
//    else
//    {
//        await _catalogContext.SaveChangesAsync();
//    }
//}

//The first DbContext is _catalogContext and the second DbContext is within the _catalogIntegrationEventService object.
//The Commit action is performed across all DbContext objects using an EF execution strategy.

//KEYPOINT 2 -- To achieve this multiple DbContext commit, the SaveEventAndCatalogContextChangesAsync uses a ResilientTransaction class, as shown in the following code:

//public class CatalogIntegrationEventService : ICatalogIntegrationEventService
//{
//    //…
//    public async Task SaveEventAndCatalogContextChangesAsync(
//        IntegrationEvent evt)
//    {
//        // Use of an EF Core resiliency strategy when using multiple DbContexts
//        // within an explicit BeginTransaction():
//        // https://learn.microsoft.com/ef/core/miscellaneous/connection-resiliency
//        await ResilientTransaction.New(_catalogContext).ExecuteAsync(async () =>
//        {
//            // Achieving atomicity between original catalog database
//            // operation and the IntegrationEventLog thanks to a local transaction
//            await _catalogContext.SaveChangesAsync();
//            await _eventLogService.SaveEventAsync(evt,
//                _catalogContext.Database.CurrentTransaction.GetDbTransaction());
//        });
//    }
//}


//KEYPOINT 3 -- The ResilientTransaction.ExecuteAsync method basically begins a transaction from the passed DbContext (_catalogContext) and then makes the EventLogService use that transaction
//to save changes from the IntegrationEventLogContext and then commits the whole transaction.

//public class ResilientTransaction
//{
//    private DbContext _context;
//    private ResilientTransaction(DbContext context) =>
//        _context = context ?? throw new ArgumentNullException(nameof(context));

//    public static ResilientTransaction New(DbContext context) =>
//        new ResilientTransaction(context);

//    public async Task ExecuteAsync(Func<Task> action)
//    {
//        // Use of an EF Core resiliency strategy when using multiple DbContexts
//        // within an explicit BeginTransaction():
//        // https://learn.microsoft.com/ef/core/miscellaneous/connection-resiliency
//        var strategy = _context.Database.CreateExecutionStrategy();
//        await strategy.ExecuteAsync(async () =>
//        {
//            await using var transaction = await _context.Database.BeginTransactionAsync();
//            await action();
//            await transaction.CommitAsync();
//        });
//    }
//}