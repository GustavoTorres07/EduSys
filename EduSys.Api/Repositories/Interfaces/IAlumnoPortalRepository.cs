using EduSys.Shared.DTOs;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IAlumnoPortalRepository
    {
        Task<List<NotificacionDTO>> GetNotificacionesAsync(int idUsuario);
        Task<bool> MarcarNotificacionLeidaAsync(int idNotificacion);
        Task<List<CursadaAlumnoDTO>> GetMisCursadasAsync(int idUsuario);
    }
}
