using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IModalidadRepository
    {
        Task<List<Modalidad>> GetAllAsync();
        Task<Modalidad?> GetByIdAsync(int id);
        Task<Modalidad> CreateAsync(Modalidad modalidad);
        Task<bool> UpdateAsync(Modalidad modalidad);
        Task<bool> DeleteAsync(int id);

        // Validación para evitar nombres duplicados
        Task<bool> ExisteNombreAsync(string nombre, int idExcluir = 0);
    }
}
