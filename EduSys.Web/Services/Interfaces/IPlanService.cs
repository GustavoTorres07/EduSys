using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IPlanService
    {
        Task<List<PlanEstudioDTO>> GetAllAsync();
        Task<PlanEstudioDTO> GetByIdAsync(int id);
        Task<int> CreateAsync(PlanEstudioDTO plan); // Retorna el ID creado
        Task<bool> UpdateAsync(PlanEstudioDTO plan);
        Task<bool> DeleteAsync(int id);
        Task<List<PlanMateriaDTO>> GetMateriasByPlanAsync(int idPlan);
        Task<bool> AgregarMateriaAsync(PlanMateriaDTO planMateria);
        Task<bool> QuitarMateriaAsync(int idPlanMateria);

        Task<bool> ActualizarCorrelativasAsync(int idPlanMateria, List<int> idsCorrelativas);
        Task<bool> EditarMateriaAsync(PlanMateriaDTO planMateria);
        Task<List<PlanMateriaDTO>> GetAllMateriasAsync();
        Task<List<PlanMateriaDTO>> GetMateriasPorSedeAsync(int idCarrera, int idSede);
    }
}
