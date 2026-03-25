using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IRolService
    {
        Task<List<RolRequestDTO>> GetAllAsync();
        Task<RolRequestDTO?> GetByIdAsync(int id);
        Task<bool> GuardarAsync(RolRequestDTO dto);
        Task<bool> EliminarAsync(int id);
        Task<List<PermisoDTO>> GetCatalogoPermisosAsync();
    }
}
