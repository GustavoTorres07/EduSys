using EduSys.Api.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace EduSys.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"] ?? "587");
            var userName = _config["EmailSettings:UserName"];
            var password = _config["EmailSettings:Password"];
            var displayName = _config["EmailSettings:DisplayName"];
            var enableSsl = bool.Parse(_config["EmailSettings:EnableSsl"] ?? "true");

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(userName!, displayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }
    }
}