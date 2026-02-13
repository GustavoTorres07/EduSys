using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface ICarreraRepository
    {
        Task<List<Carrera>> GetAllAsync();
        Task<Carrera?> GetByIdAsync(int id);
        Task<Carrera> CreateAsync(Carrera carrera);
        Task<bool> UpdateAsync(Carrera carrera);
        Task<bool> DeleteAsync(int id); // Baja lógica (desactivar)
        Task<List<int>> GetSedesIdsByCarreraAsync(int carreraId);
        Task<bool> ActualizarSedesAsync(int carreraId, List<int> idsSedes);
        Task<bool> ExisteNombreAsync(string nombre, int idExcluir = 0);
        Task<List<int>> GetModalidadesIdsByCarreraAsync(int carreraId);
        Task<bool> ActualizarModalidadesAsync(int carreraId, List<int> idsModalidades);

        Task<List<Carrera>> GetCarrerasPorSedeAsync(int idSede);
    }
}