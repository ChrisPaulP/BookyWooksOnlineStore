using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.Messaging.MassTransit;

public interface IMassTransitService
{
    Task Send<T>(T message) where T : class;
}
