using EduSys.Shared.DTOs;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IRolRepository
    {
        Task<List<RolRequestDTO>> GetAllAsync();
        Task<RolRequestDTO?> GetByIdAsync(int id);
        Task<bool> UpsertRolAsync(RolRequestDTO dto);
        Task<bool> BajaLogicaAsync(int id);
        Task<List<PermisoDTO>> GetPermisosCatalogoAsync();
    }
}
