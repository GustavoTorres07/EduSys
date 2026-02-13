namespace EduSys.Api.Services.Interfaces
{
    public interface IEmailService
    {
        // Este es el contrato que faltaba
        Task SendEmailAsync(string to, string subject, string body);
    }
}