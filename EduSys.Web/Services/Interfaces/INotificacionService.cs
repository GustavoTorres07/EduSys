using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface INotificacionApiService
    {
        Task<List<NotificacionDTO>> GetMisNotificacionesAsync();
        Task<bool> MarcarComoLeidaAsync(int id);
        Task<bool> MarcarTodasComoLeidasAsync();
        Task<bool> EnviarNotificacionMasivaAsync(NotificacionMasivaDTO request);
    }
}