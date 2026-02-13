using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IInfrastructureRepository
    {
        // --- SEDES ---
        Task<List<Sede>> GetAllSedesAsync();
        Task<Sede?> GetSedeByIdAsync(int id);
        Task<bool> CreateSedeAsync(Sede sede);
        Task<bool> UpdateSedeAsync(Sede sede);
        Task<bool> DeleteSedeAsync(int id);

        // --- AULAS ---
        Task<List<Aula>> GetAulasBySedeAsync(int idSede);
        Task<bool> CreateAulaAsync(Aula aula);
        Task<bool> UpdateAulaAsync(Aula aula);
        Task<bool> DeleteAulaAsync(int id);
    }
}
