using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IAlumnoPortalService
    {
        Task<List<NotificacionDTO>> GetNotificacionesAsync();
        Task MarcarLeidaAsync(int id);
        Task<List<CursadaAlumnoDTO>> GetMisCursadasAsync();
        Task<AlumnoDTO> GetPerfilAsync();

        // 🚀 CORRECCIÓN: Le quitamos el "int idUsuario" porque el Backend lo saca del Token
        Task<List<AsistenciaMateriaDTO>> GetMisAsistenciasAsync();
    }
}