using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IComisionService
    {
        Task<List<ComisionDTO>> GetAllAsync();
        Task<List<ComisionDTO>> GetByPeriodoAsync(int idPeriodo);
        Task<ComisionDTO?> GetByIdAsync(int id); // ✅ Tipado nulable para mayor seguridad
        Task<bool> CreateAsync(ComisionDTO dto);
        Task<bool> UpdateAsync(ComisionDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<List<ComisionDTO>> GetByPeriodoAndCarreraAsync(int idPeriodo, int idCarrera, int? idAlumno = null);
        Task<List<ComisionDTO>> GetComisionesPorSedeAsync(int idSede);

        // --- Gestión de Docentes ---
        Task<List<DocenteComisionListadoDTO>> GetDocentesPorComisionAsync(int idComision);
        Task<ResultadoOperacionDTO> AsignarDocenteAsync(DocenteComisionRequestDTO dto);
        Task<bool> DesasignarDocenteAsync(int idAsignacion);
    }
}