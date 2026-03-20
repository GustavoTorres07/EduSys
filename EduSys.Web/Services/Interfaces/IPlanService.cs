using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IPlanService
    {
        Task<List<PlanEstudioDTO>> GetAllAsync();
        Task<PlanEstudioDTO?> GetByIdAsync(int id); // ✅ Nulable para proteger la vista
        Task<int> CreateAsync(PlanEstudioDTO plan); // Retorna el ID creado o 0 si falla
        Task<bool> UpdateAsync(PlanEstudioDTO plan);
        Task<bool> DeleteAsync(int id);

        // --- Gestión de Materias dentro del Plan ---
        Task<List<PlanMateriaDTO>> GetMateriasByPlanAsync(int idPlan);
        Task<bool> AgregarMateriaAsync(PlanMateriaDTO planMateria);
        Task<bool> EditarMateriaAsync(PlanMateriaDTO planMateria);
        Task<bool> QuitarMateriaAsync(int idPlanMateria);
        Task<bool> ActualizarCorrelativasAsync(int idPlanMateria, List<CorrelativaItemDTO> correlativas);

        // --- Consultas globales y por Sede ---
        Task<List<PlanMateriaDTO>> GetAllMateriasAsync();
        Task<List<PlanMateriaDTO>> GetMateriasPorSedeAsync(int idCarrera, int idSede);

        Task<List<PlanSedeDTO>> GetSedesByPlanAsync(int idPlan);
        Task<bool> ActualizarSedesAsync(int idPlan, List<int> idsSedes);
    }
}