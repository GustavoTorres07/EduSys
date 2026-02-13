using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface ISolicitudService
    {
        // 1. Enviar formulario (Público)
        Task EnviarSolicitudAsync(SolicitudIngresoRequestDTO solicitud);

        // 2. Obtener lista de pendientes (Admin)
        Task<List<SolicitudIngresoDTO>> GetPendientesAsync();

        // 3. Obtener una solicitud por ID (Admin - Detalle) - ESTE FALTABA
        Task<SolicitudIngresoDTO> GetSolicitudByIdAsync(int id);

        // 4. Procesar Aprobación/Rechazo (Admin) - ESTE FALTABA
        Task ProcesarSolicitudAsync(ProcesarSolicitudDTO decision);

        Task<List<SolicitudIngresoDTO>> GetHistorialSolicitudes();
    }
}