using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using RentalVehicleService.Models;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace RentalVehicleService.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> opts, ILogger<SmtpEmailService> logger)
        {
            _settings = opts.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            email.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Use StartTls for port 587
                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
                
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
                await client.SendAsync(email);
                
                await client.DisconnectAsync(true);
                _logger.LogInformation("Email sent successfully to {To} via MailKit", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MailKit failed to send email to {To}. Error: {Message}", to, ex.Message);
                throw;
            }
        }
    }
}
