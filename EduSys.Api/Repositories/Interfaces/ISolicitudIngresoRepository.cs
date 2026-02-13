using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface ISolicitudIngresoRepository
    {
        // --- PARA EL ALUMNO (Formulario Público) ---
        Task<SolicitudIngreso> CrearAsync(SolicitudIngreso solicitud);
        Task<bool> ExistePendienteAsync(string dni, int idCarrera);

        Task<List<SolicitudIngreso>> GetPendientesAsync();
        // --- PARA EL ADMINISTRADOR (Gestión) ---
        Task<List<SolicitudIngreso>> GetAllAsync(); // Ver todas
        Task<SolicitudIngreso?> GetByIdAsync(int id); // Ver detalle
        Task<bool> ActualizarEstadoAsync(int id, string nuevoEstado, string? observacion); // Aprobar/Rechazar
        Task UpdateAsync(SolicitudIngreso solicitud);
        Task<List<SolicitudIngreso>> GetHistorialAsync();
    }
}