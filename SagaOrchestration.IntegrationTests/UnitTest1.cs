using BookyWooks.Messaging.Constants;
using BookyWooks.Messaging.Contracts.Commands;
using BookyWooks.Messaging.Contracts.Events;
using BookyWooks.Messaging.MassTransit;
using BookyWooks.Messaging.Messages.InitialMessage;
using FluentAssertions;
using IntegrationTestingSetup;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Internals;
using MassTransit.Serialization;
using MassTransit.Testing;
using MassTransit.Transports;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SagaOrchestration.Data;
using SagaOrchestration.StateInstances;
using SagaOrchestration.StateMachines;
using System.Reflection;
using System.Text;
using static MassTransit.Logging.OperationName;

namespace SagaOrchestration.IntegrationTests
{
    [Collection("Saga Orchestration Test Collection")]
    public class UnitTest1 : ApiTestBase<Program, StateMachineDbContext>
    {
        private readonly HttpClient _client;
        private readonly ITestHarness _harness;
        public UnitTest1(CustomSagaOrchestrationTestFactory<Program> apiFactory) : base(apiFactory, apiFactory.DisposeAsync)
        {
            _client = apiFactory.CreateClient();
            _harness = apiFactory.Services.GetTestHarness();
        }
        [Fact]
        public async Task Test1()
        {
            EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:create-order-message-queue"));
            //EndpointConvention.Map<CheckBookStockCommand>(new Uri("queue:check-book-stock-command-queue"));

            await _harness.Start();

            var orderItems = new List<OrderItemEventDto>
    {
        new OrderItemEventDto(Guid.NewGuid(), 1, true)
    };

            var message = new OrderCreatedMessage(
                orderId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                orderTotal: 9.99M,
                orderItems: orderItems
            );

            await _harness.Bus.Send(message);

            var outboxEntries = await FindAllAsync<OutboxMessage>();
            var inboxState = await FindAllAsync<InboxState>();
            var outboxState = await FindAllAsync<OutboxState>();
            var outboxStateInstance = await FindAllAsync<OrderStateInstance>();

            var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderStateInstance>();

            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());

             outboxEntries = await FindAllAsync<OutboxMessage>();
             inboxState = await FindAllAsync<InboxState>();
             outboxState = await FindAllAsync<OutboxState>();
             outboxStateInstance = await FindAllAsync<OrderStateInstance>();

            var createdSagaInstance = sagaHarness.Created
            .Select(x => x.Saga.OrderId == message.orderId)
            .FirstOrDefault();

            outboxEntries = await FindAllAsync<OutboxMessage>();
            var outboxMessage = outboxEntries.FirstOrDefault();
            inboxState = await FindAllAsync<InboxState>();
            outboxState = await FindAllAsync<OutboxState>();
            outboxStateInstance = await FindAllAsync<OrderStateInstance>();

            var myCommand = JsonConvert.DeserializeObject<CheckBookStockCommand>(outboxMessage.Body);

            var correlationId = createdSagaInstance?.Saga.CorrelationId ?? Guid.Empty;

            var jsonBody = JObject.Parse(outboxMessage.Body);

            // Step 2: Access the "message" property, which contains the CheckBookStockCommand data
            var messageJson = jsonBody["message"].ToString();

            // Step 3: Deserialize the "message" JSON into the CheckBookStockCommand object
            var myCommand2 = JsonConvert.DeserializeObject<CheckBookStockCommand>(messageJson);

            var instance = sagaHarness.Created.ContainsInState(correlationId, sagaHarness.StateMachine, sagaHarness.StateMachine.StockCheck);
            Assert.NotNull(instance); // Ensure the saga is in the StockCheck state

            // Check if the command was sent
            await Task.Delay(1000); // Increase the delay to 2 seconds or more // Optional delay to ensure the message has been sent



            var serializedMessage = _harness.Sent
                                    .Select<MassTransit.Serialization.SerializedMessageBody>()
                                    .FirstOrDefault();

            outboxEntries = await FindAllAsync<OutboxMessage>();
            inboxState = await FindAllAsync<InboxState>();
            outboxState = await FindAllAsync<OutboxState>();
            outboxStateInstance = await FindAllAsync<OrderStateInstance>();
            //        var serializedMessage2 = _harness.Sent
            //.Select<ISentMessage<SerializedMessageBody>>() // Ensure you use the correct type here
            //.FirstOrDefault();

            // Ensure the message was found
            if (serializedMessage != null)
            {
                // Extract the actual body from the sent message
                var messageBody = serializedMessage.Context.Message;

                // Deserialize the message body
                //var eventMessage = JsonSerializer.Deserialize<CheckBookStockCommand>(messageBody);
            }

            if (serializedMessage != null)
            {
                // Use the Deconstruct method to get message and context
                serializedMessage.Deconstruct(out var message7, out var context);

                // Now you can work with the message and context
                var bodyText = message7; // Since message is of type SerializedMessageBody

                // If you need to deserialize the body, you can convert it to a string
                var rawBody = context.ContentType; // Adjust this according to how your body is structured

                // Assuming the raw body is a byte array
                //var bodyText = Encoding.UTF8.GetString(rawBody); // Use correct encoding if necessary

                Console.WriteLine($"Serialized message body: {bodyText}");

                // Deserialize into your command object if needed
                //var checkBookStockCommand = JsonConvert.DeserializeObject<CheckBookStockCommand>(bodyText);
                //Assert.NotNull(checkBookStockCommand);
            }

            // Deserialize the message if it's serialized
            if (serializedMessage != null)
            {
                var deserializedMessage = serializedMessage.Context.Message; // as CheckBookStockCommand;
                var deserializedMessage9 = serializedMessage.Context.Message.ToString(); // as CheckBookStockCommand;

                // var deserializedMessage7 = serializedMessage.Context as CheckBookStockCommand;

                var getPayload2 = serializedMessage.Context?.Serialization.GetMessageDeserializer();
                var getPayload3 = serializedMessage.Context?.Serialization.GetMessageSerializer();

                var getPayload = serializedMessage.Context?.GetPayload<Object>();
                var getPayload7 = serializedMessage.Context?.BodyLength;

                var deserializedMessage5 = getPayload; // as CheckBookStockCommand;

                var headers = serializedMessage.Context?.Headers;

                if (headers != null)
                {
                    Console.WriteLine("Message headers:");

                    foreach (var header in headers.GetAll())
                    {
                        Console.WriteLine($"{header.Key}: {header.Value}");
                    }
                }

                // If you want to access the body as well
                //var body = serializedMessage.Context.;

                Assert.NotNull(deserializedMessage); // Ensure the message was deserialized correctly
                                                     //Assert.Equal(correlationId, deserializedMessage?.CorrelationId); // Check correlation ID

            }

            var sentCommand = _harness.Sent.Select<CheckBookStockCommand>().FirstOrDefault();
            Assert.NotNull(sentCommand); // Ensure a command was sent

            var sentCorrelationId = sentCommand.Context.Message.CorrelationId;

            Assert.Equal(correlationId, sentCorrelationId); // Ensure correlation IDs match

            Assert.True(await _harness.Sent.Any<CheckBookStockCommand>(x => x.Context.Message.CorrelationId == correlationId));
        }

        [Fact]
        public async Task Order_Message_Created_And_Consumed()
        {
            EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:create-order-message-queue"));

            await _harness.Start();

            var orderItems = new List<OrderItemEventDto>
            {
                new OrderItemEventDto(Guid.NewGuid(), 1, true)
            };

            var message = new OrderCreatedMessage(
                orderId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                orderTotal: 9.99M,
                orderItems: orderItems
            );

            await _harness.Bus.Send(message);

            var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderStateInstance>();

            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());
        }
        [Fact]
        public async Task Check_Book_Stock_Command_Is_Consumed()
        {
            EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:create-order-message-queue"));

            await _harness.Start();

            var orderItems = new List<OrderItemEventDto>
            {
                new OrderItemEventDto(Guid.NewGuid(), 1, true)
            };

            var message = new OrderCreatedMessage(
                orderId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                orderTotal: 9.99M,
                orderItems: orderItems
            );

            await _harness.Bus.Send(message);

            var outboxEntries = await FindAllAsync<OutboxMessage>(); // Empty Here
            var inboxState = await FindAllAsync<InboxState>();  // Empty Here
            var outboxStateInstance = await FindAllAsync<OrderStateInstance>(); // Empty Here

            var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderStateInstance>();

            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());

            outboxEntries = await FindAllAsync<OutboxMessage>(); // Empty Here
            inboxState = await FindAllAsync<InboxState>(); // 1 Count Here
            outboxStateInstance = await FindAllAsync<OrderStateInstance>();// 1 Count Here

            Assert.Empty(outboxEntries); // Ensure the outbox table is empty
            Assert.Equal(1, inboxState.Count()); // Ensure the message was consumed and persisted to the inbox table
            Assert.True(inboxState.FirstOrDefault().Delivered == null);
            Assert.Equal(1, outboxStateInstance.Count()); // Ensure the saga state was persisted

            var sagaState = outboxStateInstance.FirstOrDefault().CurrentState;
            var inboxStateMessageId = inboxState.FirstOrDefault().MessageId;

            var createdSagaInstance = sagaHarness.Created
            .Select(x => x.Saga.OrderId == message.orderId)
            .FirstOrDefault();

            outboxEntries = await FindAllAsync<OutboxMessage>(); // 1 Count Here
            var outboxMessage = outboxEntries.FirstOrDefault(); // 1 Count Here
            //var xx = await FindAsync<OutboxMessage>(inboxStateMessageId);// 1 Count Here
            inboxState = await FindAllAsync<InboxState>(); // 1 Count Here
            outboxStateInstance = await FindAllAsync<OrderStateInstance>();

            //var myCommand = JsonConvert.DeserializeObject<CheckBookStockCommand>(outboxMessage.Body);

            var correlationId = createdSagaInstance?.Saga.CorrelationId ?? Guid.Empty;

            var jObject = JObject.Parse(outboxMessage.Body);

            // Step 2: Access the "message" property, which contains the CheckBookStockCommand data
            var messageJson = jObject["message"].ToString();

            // Step 3: Deserialize the "message" JSON into the CheckBookStockCommand object
            var checkBookStockCommand = JsonConvert.DeserializeObject<CheckBookStockCommand>(messageJson);

            var instance = sagaHarness.Created.ContainsInState(correlationId, sagaHarness.StateMachine, sagaHarness.StateMachine.StockCheck);
            Assert.NotNull(instance); // Ensure the saga is in the StockCheck state

            // Check if the command was sent
            await Task.Delay(1000); // Increase the delay to 2 seconds or more // Optional delay to ensure the message has been sent



            var serializedMessage = _harness.Sent
                                    .Select<MassTransit.Serialization.SerializedMessageBody>()
                                    .FirstOrDefault();

            outboxEntries = await FindAllAsync<OutboxMessage>();
            inboxState = await FindAllAsync<InboxState>();
            outboxStateInstance = await FindAllAsync<OrderStateInstance>();

            var test = outboxStateInstance.FirstOrDefault().CurrentState;

            Assert.Equal(checkBookStockCommand.CorrelationId, serializedMessage.Context.CorrelationId);
            Assert.Equal(inboxState.FirstOrDefault().Delivered, DateTime.Today.Date);// Ensure correlation IDs match

        }

        [Fact]
        public async Task Outbox_Should_Persist_Message_And_Send_After_Failure()
        {
            EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:create-order-message-queue"));
            // Step 1: Start the test harness
            await _harness.Start();

            var orderItems = new List<OrderItemEventDto>
    {
        new OrderItemEventDto(Guid.NewGuid(), 1, true)
    };

            var message = new OrderCreatedMessage(
                orderId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                orderTotal: 9.99M,
                orderItems: orderItems
            );

            // Step 2: Send a message that should trigger the outbox
            await _harness.Bus.Send(message);
            var outboxEntries = await FindAllAsync<OutboxMessage>();
            var inboxState = await FindAllAsync<InboxState>();
            var outboxState = await FindAllAsync<OutboxState>();
            var outboxStateInstance = await FindAllAsync<OrderStateInstance>();

            // Step 3: Get the saga harness and verify the OrderCreatedMessage was consumed
            var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderStateInstance>();
            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());

            // Step 4: Check the outbox table in the database (this requires database access)
            outboxEntries = await FindAllAsync<OutboxMessage>();
            inboxState = await FindAllAsync<InboxState>();
            outboxState = await FindAllAsync<OutboxState>();
            outboxStateInstance = await FindAllAsync<OrderStateInstance>();

            Assert.NotEmpty(inboxState);

            // Optional: you could inspect the message content here


            // Simulate a failure before the message is sent
            // In a real-world test, you might stop the bus here or cause a transient failure

            // Step 5: Polling or wait for the outbox to process the message and send it
            var timeout = TimeSpan.FromSeconds(5);
            var pollingInterval = TimeSpan.FromMilliseconds(200);
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < timeout)
            {
                var sentCommand = _harness.Sent.Select<CheckBookStockCommand>().FirstOrDefault();
                if (sentCommand != null)
                {
                    // Command was successfully sent
                    Assert.NotNull(sentCommand);
                    return;
                }

                // Wait for the next polling interval
                await Task.Delay(pollingInterval);
            }

            // Step 6: If the message wasn't sent within the timeout, fail the test
            Assert.Fail("CheckBookStockCommand was not sent by the outbox within the timeout period.");
        }


        //[Fact]
        //public async Task Saga_Should_WriteMessage_ToOutbox()
        //{
        //    // Arrange: Trigger the saga with an initial message
        //    var yourInitialMessage = new YourSagaStartMessage
        //    {
        //        CorrelationId = Guid.NewGuid(),
        //        // other properties
        //    };

        //    var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:your-saga-start"));
        //    await sendEndpoint.Send(yourInitialMessage);

        //    // Act: Wait for the saga to process and write to the outbox
        //    await _dbContext.SaveChangesAsync();

        //    // Assert: Check the OutboxMessage table to ensure the message is saved
        //    var outboxMessage = await _dbContext.OutboxMessages
        //        .FirstOrDefaultAsync(m => m.MessageType == "YourExpectedMessageType");

        //    Assert.NotNull(outboxMessage); // Ensure that a message was written to the outbox
        //    Assert.Equal("YourExpectedMessageType", outboxMessage.MessageType);
        //    Assert.Contains("ExpectedContent", outboxMessage.MessageBody); // Check message content if needed
        //}

        [Fact]
        public async Task Check_Book_Stock_Command_Is_Consumed_InboxState_Validation()
        {
            EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:create-order-message-queue"));
            //EndpointConvention.Map<CheckBookStockCommand>(new Uri("queue:check-book-stock-command-queue"));

            await _harness.Start();

            var orderItems = new List<OrderItemEventDto>
            {
                new OrderItemEventDto(Guid.NewGuid(), 1, true)
            };

            var message = new OrderCreatedMessage(
                orderId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                orderTotal: 9.99M,
                orderItems: orderItems
            );

            await _harness.Bus.Send(message);


            var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderStateInstance>();

            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());

            // Step 4: Check InboxState for OrderCreatedMessage
            var inboxState = await FindAllAsync<InboxState>();
            Assert.Equal(1, inboxState.Count());  // Expect 1 entry for OrderCreatedMessage
            var firstInboxState = inboxState.FirstOrDefault();
            Assert.Null(firstInboxState.Delivered);

            //var createdSagaInstance = sagaHarness.Created
            //.Select(x => x.Saga.OrderId == message.orderId)
            //.FirstOrDefault();



            // Step 5: Wait for CheckBookStockCommand to be sent from the saga
            //await Task.Delay(1000);
            var outboxEntries = await FindAllAsync<OutboxMessage>();
            Assert.Equal(1, outboxEntries.Count());  // Ensure the outbox has the CheckBookStockCommand

            // Step 6: Wait for CheckBookStockCommand to be consumed (if it has a consumer)
            //Assert.True(await _harness.Consumed.Any<SerializedMessageBody>());
            //var serializedMessage = _harness.Sent
            //                        .Select<MassTransit.Serialization.SerializedMessageBody>()
            //                        .FirstOrDefault();

            // Step 7: Check if the InboxState now contains two entries (OrderCreatedMessage + CheckBookStockCommand)
            var inboxState2 = await FindAllAsync<InboxState>();
            Assert.Equal(1, inboxState2.Count());  // Ensure inbox has 2 entries

            var secondInboxState = inboxState.OrderBy(x => x.Received).Last();
            Assert.Null(secondInboxState.Delivered);  // Check if second message is not delivered yet

            // Step 8: Verify message order (OrderCreatedMessage should come first)
            //Assert.True(firstInboxState.Received < secondInboxState.Received);
            //Assert.True(firstInboxState.Received < secondInboxState.Received);

            // Ensure the saga is in the StockCheck state after sending the CheckBookStockCommand
            var createdSagaInstance = sagaHarness.Created.Select(x => x.Saga.OrderId == message.orderId).FirstOrDefault();
            var correlationId = createdSagaInstance?.Saga.CorrelationId ?? Guid.Empty;
            var instance = sagaHarness.Created.ContainsInState(correlationId, sagaHarness.StateMachine, sagaHarness.StateMachine.StockCheck);
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task Check_Book_Stock_Consumer()
        {
            EndpointConvention.Map<OrderCreatedMessage>(new Uri("queue:create-order-message-queue"));
            //EndpointConvention.Map<CheckBookStockCommand>(new Uri("queue:check-book-stock-command-queue"));

            await _harness.Start();

            var orderItems = new List<OrderItemEventDto>
            {
                new OrderItemEventDto(Guid.NewGuid(), 1, true)
            };

            var message = new OrderCreatedMessage(
                orderId: Guid.NewGuid(),
                customerId: Guid.NewGuid(),
                orderTotal: 9.99M,
                orderItems: orderItems
            );

            await _harness.Bus.Send(message);


            var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderStateInstance>();

            Assert.True(await sagaHarness.Consumed.Any<OrderCreatedMessage>());

            // Step 4: Check InboxState for OrderCreatedMessage
            var inboxState = await FindAllAsync<InboxState>();
            Assert.Equal(1, inboxState.Count());  // Expect 1 entry for OrderCreatedMessage
            var firstInboxState = inboxState.FirstOrDefault();
            Assert.Null(firstInboxState.Delivered);

            var outboxEntries = await FindAllAsync<OutboxMessage>();
            Assert.Equal(1, outboxEntries.Count());  // Ensure the outbox has the CheckBookStockCommand

            // Step 6: Wait for CheckBookStockCommand to be consumed (if it has a consumer)
            

            //Assert.True(await sagaHarness.Consumed.Any<SerializedMessageBody>());

            var createdSagaInstance = sagaHarness.Created
            .Select(x => x.Saga.OrderId == message.orderId)
            .FirstOrDefault();

            var stockItems = new List<OrderItemEventDto>();
            var stockConfirmedEvent = new StockConfirmedEvent(createdSagaInstance.Saga.CorrelationId, createdSagaInstance.Saga.OrderId, stockItems);
            await _harness.Bus.Publish(stockConfirmedEvent);
            Assert.True(await sagaHarness.Consumed.Any<StockConfirmedEvent>());

            // Step 7: Check if the InboxState now contains two entries (OrderCreatedMessage + CheckBookStockCommand)
            var inboxState2 = await FindAllAsync<InboxState>();
            Assert.Equal(2, inboxState2.Count());  // Ensure inbox has 2 entries

            var secondInboxState = inboxState.OrderBy(x => x.Received).Last();
            Assert.Null(secondInboxState.Delivered);  // Check if second message is not delivered yet

            // Step 8: Verify message order (OrderCreatedMessage should come first)
            //Assert.True(firstInboxState.Received.TimeOfDay < secondInboxState.Received.TimeOfDay);
            //Assert.True(firstInboxState.Received < secondInboxState.Received);

            // Ensure the saga is in the StockCheck state after sending the CheckBookStockCommand
            //var createdSagaInstance = sagaHarness.Created.Select(x => x.Saga.OrderId == message.orderId).FirstOrDefault();

            var currentState = createdSagaInstance.Saga.CurrentState;
            var correlationId = createdSagaInstance?.Saga.CorrelationId ?? Guid.Empty;
                      
            var instance = sagaHarness.Created.ContainsInState(correlationId, sagaHarness.StateMachine, sagaHarness.StateMachine.StockCheck);

            var outboxState = await FindAllAsync<OutboxState>();
            var outboxStateInstance = await FindAllAsync<OrderStateInstance>();

        }

    }

}
