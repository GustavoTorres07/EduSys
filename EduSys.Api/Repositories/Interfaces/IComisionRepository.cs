using EduSys.Shared.DTOs;
using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IComisionRepository
    {
        // --- CRUD BÁSICO ---
        Task<List<Comision>> GetAllAsync();
        Task<List<Comision>> GetByPeriodoAsync(int idPeriodo);
        Task<Comision?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Comision comision);
        Task<bool> UpdateAsync(Comision comision);
        Task<bool> DeleteAsync(int id); // Baja lógica (Estado = "Cancelada")

        // --- CONSULTAS ESPECÍFICAS ---
        Task<List<Comision>> GetByPeriodoAndCarreraAsync(int idPeriodo, int idCarrera);

        // --- DOCENTES ---
        Task<bool> AsignarDocenteAsync(DocenteComisionRequestDTO dto);
        Task<bool> DesasignarDocenteAsync(int idDocenteComision);
        Task<List<DocenteComisionListadoDTO>> GetDocentesPorComisionAsync(int idComision);

        // --- DTO OPTIMIZADO PARA FRONT ---
        Task<List<ComisionDTO>> GetDTOByPeriodoAndCarreraAsync(int idPeriodo, int idCarrera);

        Task<List<ComisionDTO>> GetPorSedeAsync(int idSede);

    }
}
