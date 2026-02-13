using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IPlanEstudioRepository
    {
        Task<List<PlanEstudio>> GetAllAsync();
        Task<PlanEstudio?> GetByIdAsync(int id);
        Task<PlanEstudio> CreateAsync(PlanEstudio plan);
        Task<bool> UpdateAsync(PlanEstudio plan);
        Task<bool> DeleteAsync(int id); // Baja lógica (EsVigente = false)

        // Métodos para el detalle (Materias del plan)
        Task<List<PlanMateria>> GetMateriasByPlanAsync(int idPlan);
        Task<bool> AgregarMateriaAsync(PlanMateria planMateria);
        Task<bool> QuitarMateriaAsync(int idPlanMateria);
        Task<bool> ActualizarCorrelativasAsync(int idPlanMateriaOrigen, List<int> idsPlanMateriaRequisitos);
        Task<bool> ModificarMateriaDelPlanAsync(PlanMateria planMateria); // <--- AGREGAR ESTO

        Task<List<PlanMateria>> GetAllMateriasGlobalAsync();
    }
}
