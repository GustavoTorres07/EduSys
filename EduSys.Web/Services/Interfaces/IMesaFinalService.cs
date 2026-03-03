using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IMesaFinalService
    {
        Task<List<MesaFinalDTO>> GetAllAsync();
        Task<List<MesaFinalDTO>> GetByPeriodoAsync(int idPeriodo);
        Task<ResultadoOperacionDTO> CreateAsync(MesaFinalRequestDTO dto);
        Task<ResultadoOperacionDTO> UpdateAsync(MesaFinalRequestDTO dto);
        Task<ResultadoOperacionDTO> DeleteAsync(int id);
    }
}
