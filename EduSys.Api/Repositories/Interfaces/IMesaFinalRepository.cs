using EduSys.Shared.DTOs;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IMesaFinalRepository
    {
        Task<List<MesaFinalDTO>> GetAllAsync();
        Task<List<MesaFinalDTO>> GetByPeriodoAsync(int idPeriodo);
        Task<MesaFinalDTO?> GetByIdAsync(int id);
        Task<ResultadoOperacionDTO> CreateAsync(MesaFinalRequestDTO dto);
        Task<ResultadoOperacionDTO> UpdateAsync(MesaFinalRequestDTO dto);
        Task<ResultadoOperacionDTO> DeleteAsync(int id);
    }
}
