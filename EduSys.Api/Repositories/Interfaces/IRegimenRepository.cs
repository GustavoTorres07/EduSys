using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IRegimenRepository
    {
        Task<List<Regimen>> GetAllAsync();
        Task<Regimen?> GetByIdAsync(int id);
        Task<Regimen> CreateAsync(Regimen regimen);
        Task<bool> UpdateAsync(Regimen regimen);
        Task<bool> DeleteAsync(int id);
    }
}
