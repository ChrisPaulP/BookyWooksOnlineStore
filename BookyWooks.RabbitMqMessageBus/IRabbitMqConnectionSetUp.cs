using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.RabbitMqMessageBus;

public interface IRabbitMqConnectionSetUp //: IDisposable
{
    bool IsConnected { get; }
    IModel CreateModel();
}
