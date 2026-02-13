using EduSys.Shared.Models; // O DTO si tienes AulaDTO

namespace EduSys.Web.Services.Interfaces
{
    public interface IAulaService
    {
        // Método clave: Traer aulas filtradas por sede
        Task<List<Aula>> GetBySedeAsync(int idSede);
    }
}