using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IPeriodoService
    {
        Task<List<PeriodoAcademicoDTO>> GetAllAsync();
        Task<PeriodoAcademicoDTO?> GetByIdAsync(int id); // ✅ Nulable para mayor seguridad
        Task<bool> CreateAsync(PeriodoAcademicoDTO dto);
        Task<bool> UpdateAsync(PeriodoAcademicoDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}