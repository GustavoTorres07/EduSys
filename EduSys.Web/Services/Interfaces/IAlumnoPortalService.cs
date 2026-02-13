using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IAlumnoPortalService
    {
        Task<List<NotificacionDTO>> GetNotificacionesAsync();
        Task MarcarLeidaAsync(int id);
        Task<List<CursadaAlumnoDTO>> GetMisCursadasAsync();
    }
}
