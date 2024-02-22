namespace BookWooks.OrderApi.Core.Interfaces;

public interface IEmailSender
{
  //Task SendEmailAsync(string to, string from, string subject, string body);
  Task SendEmailAsync(string to, string from, string subject, string body); 
}
