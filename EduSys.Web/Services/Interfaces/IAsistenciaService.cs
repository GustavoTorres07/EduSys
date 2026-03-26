using EduSys.Shared.DTOs;
using System.Threading.Tasks;

namespace EduSys.Web.Services.Interfaces
{
    public interface IAsistenciaService
    {
        Task<AsistenciaGrillaDTO> GetGrillaByComisionAsync(int idComision);
        Task<bool> GuardarGrillaAsync(GuardarAsistenciaRequestDTO request);
        Task<string?> SubirCertificadoAsync(string base64Content, string fileName);
    }
}