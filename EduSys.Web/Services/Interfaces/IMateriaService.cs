using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IMateriaService
    {
        Task<List<MateriaDTO>> GetAllAsync();
        Task<MateriaDTO?> GetByIdAsync(int id); // ✅ Nulable para mayor seguridad
        Task<bool> CreateAsync(MateriaDTO materia);
        Task<bool> UpdateAsync(MateriaDTO materia);
        Task<bool> DeleteAsync(int id);
    }
}