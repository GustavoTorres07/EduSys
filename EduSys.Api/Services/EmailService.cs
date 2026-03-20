using EduSys.Api.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace EduSys.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger; // ✅ Inyectamos el sistema de logs

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var host = _config["EmailSettings:Host"];

                // Manejo seguro de conversiones (evita crasheos si el appsettings está mal escrito)
                int port = int.TryParse(_config["EmailSettings:Port"], out int parsedPort) ? parsedPort : 587;
                bool enableSsl = bool.TryParse(_config["EmailSettings:EnableSsl"], out bool parsedSsl) ? parsedSsl : true;

                var userName = _config["EmailSettings:UserName"];
                var password = _config["EmailSettings:Password"];
                var displayName = _config["EmailSettings:DisplayName"] ?? "EduSys";

                // Validación temprana de credenciales
                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("⚠️ ATENCIÓN: Credenciales SMTP incompletas en appsettings.json. El correo a {To} no se enviará.", to);
                    return;
                }

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(userName, password),
                    EnableSsl = enableSsl
                };

                // ✅ Agregamos 'using' porque MailMessage también consume recursos no administrados
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(userName, displayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("✅ Correo enviado exitosamente a {To} con el asunto '{Subject}'.", to, subject);
            }
            catch (SmtpException smtpEx)
            {
                // Capturamos específicamente errores del servidor de correos (ej: credenciales inválidas)
                _logger.LogError(smtpEx, "❌ Error SMTP al enviar correo a {To}. Código de estado: {StatusCode}", to, smtpEx.StatusCode);
                throw; // Lanzamos la excepción para que el controlador que lo llamó sepa que falló
            }
            catch (Exception ex)
            {
                // Capturamos cualquier otro error inesperado
                _logger.LogError(ex, "❌ Error general al intentar enviar correo a {To}.", to);
                throw;
            }
        }
    }
}