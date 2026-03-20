using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface ISedeService
    {
        // --- Gestión de Sedes ---
        Task<List<SedeDTO>> GetAllAsync();
        Task<SedeDTO?> GetByIdAsync(int id); // ✅ Nulable para mayor seguridad
        Task<bool> CreateAsync(SedeDTO sede);
        Task<bool> UpdateAsync(SedeDTO sede);
        Task<bool> DeleteAsync(int id);

        // --- Gestión de Aulas ---
        Task<List<AulaDTO>> GetAulasBySedeAsync(int idSede);
        Task<bool> CreateAulaAsync(AulaDTO aula);
        Task<bool> UpdateAulaAsync(AulaDTO aula);
        Task<bool> DeleteAulaAsync(int id);
    }
}