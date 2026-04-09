using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface INotasService
    {
        Task<PlanillaNotasDTO?> GetPlanillaAsync(int idComision);
        Task<bool> GuardarNotaAsync(int idInscripcion, int idEvaluacion, decimal? valor);
        Task<bool> CrearEvaluacionAsync(int idComision, EvaluacionDTO evaluacion);
        Task<bool> EditarEvaluacionAsync(EvaluacionDTO evaluacion);
        Task<bool> EliminarEvaluacionAsync(int idEvaluacion);
    }
}