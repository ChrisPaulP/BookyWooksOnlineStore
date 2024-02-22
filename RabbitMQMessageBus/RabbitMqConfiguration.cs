using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQMessageBus;

public class RabbitMqConfiguration
{
    public string SubscriptionClientName { get; set; }

    public string EventBusConnection { get; set; }

    public string EventBusUserName { get; set; }
    public string EventBusPassword { get; set; }
    public string EventHostName{ get; set; }

    public int EventBusRetryCount { get; set; }
    public string QueueNameForAzureFunction { get; set; }
}

