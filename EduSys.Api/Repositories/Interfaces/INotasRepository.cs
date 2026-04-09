using EduSys.Shared.DTOs;
using System.Threading.Tasks;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface INotasRepository
    {
        Task<PlanillaNotasDTO> GetPlanillaAsync(int idComision);
        Task<bool> GuardarNotaAsync(int idInscripcion, int idEvaluacion, decimal? valor);
        Task<bool> CrearEvaluacionAsync(int idComision, EvaluacionDTO evaluacion);
        Task<bool> EditarEvaluacionAsync(EvaluacionDTO dto);
        Task<bool> EliminarEvaluacionAsync(int idEvaluacion);
    }
}