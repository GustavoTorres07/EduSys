using EduSys.Shared.DTOs;
namespace EduSys.Web.Services.Interfaces
{
    public interface ISedeService
    {
        Task<List<SedeDTO>> GetAllAsync();
        Task<SedeDTO> GetByIdAsync(int id);
        Task<bool> CreateAsync(SedeDTO sede);
        Task<bool> UpdateAsync(SedeDTO sede);
        Task<bool> DeleteAsync(int id);

        Task<List<AulaDTO>> GetAulasBySedeAsync(int idSede);
        Task<bool> CreateAulaAsync(AulaDTO aula);
        Task<bool> UpdateAulaAsync(AulaDTO aula);
        Task<bool> DeleteAulaAsync(int id);
    }
}