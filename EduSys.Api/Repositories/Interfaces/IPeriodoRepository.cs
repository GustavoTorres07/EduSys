using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IPeriodoRepository
    {
        Task<List<PeriodoAcademico>> GetAllAsync();
        Task<PeriodoAcademico?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PeriodoAcademico periodo);
        Task<bool> UpdateAsync(PeriodoAcademico periodo);
        Task<bool> DeleteAsync(int id); // Baja lógica
        Task<bool> ValidarSuperposicionAsync(DateTime inicio, DateTime fin, int idExcluir = 0);
    }
}

