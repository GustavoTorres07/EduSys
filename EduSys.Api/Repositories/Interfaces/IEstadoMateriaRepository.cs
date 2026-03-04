using EduSys.Shared.DTOs;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IEstadoMateriaRepository
    {
        Task<List<EstadoMateriaDTO>> ObtenerTodosAsync();
        Task<EstadoMateriaDTO?> ObtenerPorIdAsync(int id);
        Task<EstadoMateriaDTO> CrearAsync(EstadoMateriaDTO dto);
        Task<bool> ActualizarAsync(EstadoMateriaDTO dto);
        Task<bool> EliminarAsync(int id);
    }
}
