namespace BookWooks.OrderApi.Infrastructure.Email;
public class SmtpEmailSender : IEmailSender
{
  private readonly ILogger<SmtpEmailSender> _logger;

  private readonly BookyWooksOrderDbContext _orderDbContext;

  public SmtpEmailSender(ILogger<SmtpEmailSender> logger, BookyWooksOrderDbContext orderDbContext)
  {
    _logger = logger;
  
    _orderDbContext = orderDbContext;
  }

  public async Task SendEmailAsync(string to, string from, string subject, string body)
  {
    var currentTransaction = _orderDbContext.Database.CurrentTransaction;
    if (currentTransaction == null) throw new ArgumentNullException(nameof(currentTransaction));

    //await _eventLogService.SaveEventAsync(new EmailSentIntegrationEvent(to, from, subject, body), currentTransaction);

    var emailClient = new SmtpClient("localhost");
    var message = new MailMessage
    {
      From = new MailAddress(from),
      Subject = subject,
      Body = body
    };
    message.To.Add(new MailAddress(to));
    await emailClient.SendMailAsync(message);
    _logger.LogWarning("Sending email to {to} from {from} with subject {subject}.", to, from, subject);
  }
}
