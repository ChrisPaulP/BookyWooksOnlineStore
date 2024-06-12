using BookyWooks.Messaging.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.Messaging.Messages.Events;

public record class UserCreatedEvent(string UserId, string Email, string UserName) : MessageContract;
