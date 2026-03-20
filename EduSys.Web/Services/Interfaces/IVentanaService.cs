using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IVentanaService
    {
        Task<List<VentanaOperativaDTO>> GetAllAsync();
        Task<bool> CreateAsync(VentanaOperativaDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}