using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IAulaRepository
    {
        Task<List<Aula>> GetBySedeAsync(int idSede);
    }
}
