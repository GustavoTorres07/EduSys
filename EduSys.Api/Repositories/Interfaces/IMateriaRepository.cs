using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IMateriaRepository
    {
        Task<List<Materia>> GetAllAsync();
        Task<Materia?> GetByIdAsync(int id);
        Task<Materia> CreateAsync(Materia materia);
        Task<bool> UpdateAsync(Materia materia);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExisteCodigoAsync(string codigo, int idExcluir = 0);
    }
}
