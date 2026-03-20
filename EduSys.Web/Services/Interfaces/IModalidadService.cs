using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IModalidadService
    {
        Task<List<ModalidadDTO>> GetAllAsync();
        Task<ModalidadDTO?> GetByIdAsync(int id); // ✅ Nulable para evitar excepciones si falla
        Task<bool> CreateAsync(ModalidadDTO modalidad);
        Task<bool> UpdateAsync(ModalidadDTO modalidad);
        Task<bool> DeleteAsync(int id);
    }
}