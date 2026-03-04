using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IEstadoMateriaService
    {
        Task<List<EstadoMateriaDTO>> GetEstadosAsync();
        Task<EstadoMateriaDTO?> GetEstadoByIdAsync(int id);
        Task<bool> CrearEstadoAsync(EstadoMateriaDTO dto);
        Task<bool> EditarEstadoAsync(int id, EstadoMateriaDTO dto);
        Task<bool> EliminarEstadoAsync(int id);
    }
}
