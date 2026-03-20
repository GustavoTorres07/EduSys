using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IRegimenService
    {
        Task<List<RegimenDTO>> GetAllAsync();
        Task<bool> CreateAsync(RegimenDTO regimen);
        Task<bool> UpdateAsync(RegimenDTO regimen);
        Task<bool> DeleteAsync(int id);
    }
}