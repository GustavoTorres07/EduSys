using EduSys.Shared.DTOs;
using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IPlanEstudioRepository
    {
        Task<List<PlanEstudioDTO>> GetAllAsync();
        Task<PlanEstudioDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(PlanEstudioDTO dto);
        Task<bool> UpdateAsync(PlanEstudioDTO dto);
        Task<bool> DeleteAsync(int id);

        // --- MATERIAS ---
        // IMPORTANTE: Asegúrate de que retorne DTOs, no Modelos
        Task<List<PlanMateriaDTO>> GetMateriasByPlanAsync(int idPlan);
        Task<bool> AgregarMateriaAsync(PlanMateria planMateria);
        Task<bool> QuitarMateriaAsync(int idPlanMateria);
        Task<bool> ModificarMateriaDelPlanAsync(PlanMateria pm);
        Task<bool> ActualizarCorrelativasAsync(int idPlanMateria, List<CorrelativaItemDTO> correlativas);
        Task<List<PlanMateria>> GetAllMateriasGlobalAsync();
    }
}
