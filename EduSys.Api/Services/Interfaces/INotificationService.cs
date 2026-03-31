using EduSys.Shared.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EduSys.Api.Services.Interfaces
{
    public interface INotificationService
    {
        Task NotificarCierreActaAsync(int idEvaluacion, string nombreExamen);
        Task<List<NotificacionDTO>> GetNotificacionesByUsuarioAsync(int idUsuario);
        Task<bool> MarcarLeidaAsync(int idNotificacion, int idUsuario);
        Task<bool> MarcarTodasLeidasAsync(int idUsuario);

        Task<bool> EnviarNotificacionMasivaAsync(NotificacionMasivaDTO request);

        // 🚀 NUEVOS MÉTODOS DE NEGOCIO
        Task GenerarAlertasVencimientoMesasAsync();
        Task NotificarAperturaInscripcionMateriasAsync(string periodoNombre);
    }
}