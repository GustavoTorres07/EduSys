using EduSys.Shared.DTOs;
using System.Threading.Tasks;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IAsistenciaRepository
    {
        Task<AsistenciaGrillaDTO> GetGrillaByComisionAsync(int idComision);
        Task<bool> GuardarGrillaAsync(GuardarAsistenciaRequestDTO request);
    }
}