
using EventBus.IntegrationEventInterfaceAbstraction;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OutBoxPattern
{
    public class IntegrationEventLog
    {
        private IntegrationEventLog() { }
        public IntegrationEventLog(IntegrationEventBase @integrationEventBase, Guid transactionId)
        {
            EventId = @integrationEventBase.Id;
            CreationTime = @integrationEventBase.CreatedAt;
            EventTypeName = @integrationEventBase.GetType().Name;
            EventData = JsonSerializer.Serialize(@integrationEventBase, @integrationEventBase.GetType(), new JsonSerializerOptions
            {
                WriteIndented = true
            });
            IntegrationEventLogStatus = IntegrationEventLogStatus.NotPublished;
            TimesSent = 0;
            TransactionId = transactionId.ToString();
        }
        public Guid EventId { get; private set; }
        public string? EventData { get; set; }
        public DateTime CreatedAt { get; set; }
        [NotMapped]
        public string EventTypeShortName => EventTypeName.Split('.')?.Last();
        [NotMapped]
        public IntegrationEventBase IntegrationEventBase{ get; set; }
        public IntegrationEventLogStatus IntegrationEventLogStatus { get; set; }
        public string TransactionId { get; private set; }
        public string EventTypeName { get; private set; }
        public DateTime CreationTime { get; private set; }
        public int TimesSent { get; set; }

        public IntegrationEventLog DeserializeJsonEventData(Type type)
        {
            IntegrationEventBase = JsonSerializer.Deserialize(EventData, type, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) as IntegrationEventBase;
            return this;
        }
    }
}
