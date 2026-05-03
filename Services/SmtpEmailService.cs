using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using RentalVehicleService.Models;

namespace RentalVehicleService.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        public SmtpEmailService(IOptions<EmailSettings> opts) => _settings = opts.Value;
        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var msg = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            msg.To.Add(to);
            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.Password),
                EnableSsl = true,
            };
            await client.SendMailAsync(msg);
        }
    }
}
