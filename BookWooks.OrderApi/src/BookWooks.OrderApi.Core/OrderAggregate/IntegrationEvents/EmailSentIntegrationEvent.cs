using BookyWooks.Messaging.Events;

namespace BookWooks.OrderApi.Core.OrderAggregate.IntegrationEvents;
public record EmailSentIntegrationEvent: IntegrationEvent
{
  public EmailSentIntegrationEvent(string to, string from, string subject, string body)
  {
    To = to;
    From = from;
    Subject = subject;
    Body = body;
    
  }
  public string To { get; set; }
  public string From { get; set; }
  public string Subject { get; set; }
  public string Body { get; set; }
}
