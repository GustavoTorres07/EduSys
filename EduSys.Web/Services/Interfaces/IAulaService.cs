using EduSys.Shared.DTOs; // ✅ Usamos DTO en lugar del Modelo base

namespace EduSys.Web.Services.Interfaces
{
    public interface IAulaService
    {
        Task<List<AulaDTO>> GetBySedeAsync(int idSede);
    }
}