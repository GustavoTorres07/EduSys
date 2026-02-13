using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface ICarreraService
    {
        Task<List<CarreraDTO>> GetAllAsync();
        Task<CarreraDTO> GetByIdAsync(int id);
        Task<string> CreateAsync(CarreraDTO carrera);
        Task<string> UpdateAsync(CarreraDTO carrera);
        Task<bool> DeleteAsync(int id);
        Task<List<int>> GetSedesIdsAsync(int carreraId);
        Task<bool> ActualizarSedesAsync(int carreraId, List<int> idsSedes);
        Task<List<int>> GetModalidadesIdsAsync(int carreraId);
        Task<bool> ActualizarModalidadesAsync(int carreraId, List<int> idsModalidades);
        Task<List<CarreraDTO>> GetCarrerasPorSedeAsync(int idSede);

    }
}
