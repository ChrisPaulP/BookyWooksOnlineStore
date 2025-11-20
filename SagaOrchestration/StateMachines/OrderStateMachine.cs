namespace SagaOrchestration.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
{
    private readonly ILogger _logger;
    public Event<OrderCreatedMessage> OrderCreatedEvent { get; set; }
    public Event<StockConfirmedEvent> StockConfirmedEvent { get; set; }
    public Event<StockReservationFailedEvent> StockReservationFailedEvent { get; set; }

    public State StockCheck { get; set; }
    public State StockReserved { get; set; }
    public State StockReservationFailed { get; set; }

    public OrderStateMachine() //ILogger<OrderStateMachine> logger)
    {
        _logger = Serilog.Log.Logger;
        InstanceState(x => x.CurrentState);

        Event(() => OrderCreatedEvent, y => y.CorrelateBy<Guid>(x => x.OrderId, z => z.Message.orderId)
            .SelectId(context => Guid.NewGuid()));
        Event(() => StockConfirmedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
        Event(() => StockReservationFailedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

        Initially(
            When(OrderCreatedEvent)
                .Then(context => { _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("CreateOrderMessage received in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .Then(context =>
                {
                    context.Saga.CustomerId = context.Message.customerId;
                    context.Saga.OrderId = context.Message.orderId;
                    context.Saga.CreatedDate = DateTime.UtcNow;
                    context.Saga.OrderTotal = context.Message.orderTotal;
                })
                .Send(new Uri($"queue:{QueueConstants.CheckBookStockCommandQueueName}"),
                    context => new CheckBookStockCommand(context.Saga.CorrelationId, context.Message.orderId, context.Message.customerId, context.Message.orderTotal, context.Message.orderItems))
                .TransitionTo(StockCheck)
                .Then(context => { _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("CheckBookStockCommand published in OrderStateMachine: {ContextSaga} ", context.Saga); }));

        During(StockCheck,
            When(StockConfirmedEvent)
                .Then(context => { 
                    _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("StockConfirmedEvent received in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{QueueConstants.CompletePaymentCommandQueueName}"),
                    context => new CompletePaymentCommand(context.Saga.CorrelationId, context.Saga.CustomerId, context.Saga.OrderTotal))
                .Then(context => { 
                    _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("CompletePaymentMessage sent in OrderStateMachine: {ContextSaga} ", context.Saga); }),
            When(StockReservationFailedEvent)
                .Then(context => { _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("StockReservationFailedEvent received in OrderStateMachine: {ContextSaga} ", context.Saga); })
                .TransitionTo(StockReservationFailed)
                .Publish(
                    context => new OrderFailedEvent(context.Saga.OrderId, context.Saga.CustomerId, context.Message.ErrorMessage))
                .Finalize()
                //.Then(context => { _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("OrderFailedEvent published in OrderStateMachine: {ContextSaga} ", context.Saga); })
        );

        //During(StockReserved,
        //    When(PaymentCompletedEvent)
        //        .Then(context => { _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("PaymentCompletedEvent received in OrderStateMachine: {ContextSaga} ", context.Saga); })
        //        .TransitionTo(PaymentCompleted)
        //        .Publish(
        //            context => new OrderCompletedEvent
        //            {
        //                OrderId = context.Saga.OrderId,
        //                CustomerId = context.Saga.CustomerId
        //            })
        //        .Then(context => { _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("OrderCompletedEvent published in OrderStateMachine: {ContextSaga} ", context.Saga); })
        //        .Finalize(),
        //    When(PaymentFailedEvent)
        //        .Then(context => { _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("PaymentFailedEvent received in OrderStateMachine: {ContextSaga} ", context.Saga); })
        //        .Publish(context => new OrderFailedEvent
        //        {
        //            OrderId = context.Saga.OrderId,
        //            CustomerId = context.Saga.CustomerId,
        //            ErrorMessage = context.Message.ErrorMessage
        //        })
        //        .Then(context => { _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("OrderFailedEvent published in OrderStateMachine: {ContextSaga} ", context.Saga); })
        //        .Send(new Uri($"queue:{QueuesConsts.StockRollBackMessageQueueName}"),
        //            context => new StockRollbackMessage
        //            {
        //                OrderItemList = context.Message.OrderItemList
        //            })
        //        .Then(context => { _logger.ForContext("CorrelationId", context.Saga.CorrelationId).Information("StockRollbackMessage sent in OrderStateMachine: {ContextSaga} ", context.Saga); })
        //        .TransitionTo(PaymentFailed)
        //);
    }
}
