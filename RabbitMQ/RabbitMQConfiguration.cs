using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ;

public class RabbitMQConfiguration
{
    public string HostName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; }
    public int Port { get; set; }
}

public class RabbitMQAdditionalConfiguration
{
    public int Retry { get; set; }
    public string QueueName { get; set; }
    public string BrokerName { get; set; }
}
