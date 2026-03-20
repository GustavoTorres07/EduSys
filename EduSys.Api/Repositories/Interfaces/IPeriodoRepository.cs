using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IPeriodoRepository
    {
        Task<List<PeriodoAcademico>> GetAllAsync();
        Task<PeriodoAcademico?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PeriodoAcademico periodo);
        Task<bool> UpdateAsync(PeriodoAcademico periodo);
        Task<bool> DeleteAsync(int id);
        // 🚀 Ajustado a DateOnly para coincidir con el Modelo
        Task<bool> ValidarSuperposicionAsync(DateOnly inicio, DateOnly fin, int idExcluir = 0);
    }
}