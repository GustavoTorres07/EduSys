using EduSys.Shared.DTOs;
using System.Threading.Tasks;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface INotasRepository
    {
        // Trae toda la matriz de alumnos y notas de una comisión
        Task<PlanillaNotasDTO> GetPlanillaAsync(int idComision);

        // Guarda o actualiza una nota individual
        Task<bool> GuardarNotaAsync(int idInscripcion, int idEvaluacion, decimal? valor);
        // Crea una nueva columna (Evaluación) en la planilla
        Task<bool> CrearEvaluacionAsync(int idComision, EvaluacionDTO evaluacion);

        Task<bool> EditarEvaluacionAsync(EvaluacionDTO dto);

        Task<bool> CerrarActaAsync(CierreActaDTO dto);

        Task<bool> ReabrirActaAsync(int idEvaluacion);
    }
}