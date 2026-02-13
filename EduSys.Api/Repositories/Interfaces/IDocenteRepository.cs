using EduSys.Shared.DTOs;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IDocenteRepository
    {
        Task<List<DocenteListadoDTO>> GetAllAsync();

        Task<DocenteRequestDTO?> GetByIdAsync(int id);

        Task<bool> CrearAsync(DocenteRequestDTO dto);

        Task<bool> EditarAsync(DocenteRequestDTO dto);

        Task<bool> EliminarAsync(int id); 

        Task<bool> ExisteLegajoAsync(string legajo);

        Task<List<ComisionDocenteDTO>> GetMisComisionesAsync(int idUsuario);

    }
}
