using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class NotasService : INotasService
    {
        private readonly HttpClient _http;
        private readonly ILogger<NotasService> _logger;

        public NotasService(HttpClient http, ILogger<NotasService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<PlanillaNotasDTO?> GetPlanillaAsync(int idComision)
        {
            try
            {
                return await _http.GetFromJsonAsync<PlanillaNotasDTO>($"api/Notas/planilla/{idComision}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la planilla de notas para la comisión {IdComision}.", idComision);
                return null;
            }
        }

        public async Task<bool> GuardarNotaAsync(int idInscripcion, int idEvaluacion, decimal? valor)
        {
            try
            {
                var dto = new GuardarNotaDTO { IdInscripcion = idInscripcion, IdEvaluacion = idEvaluacion, Valor = valor };
                var response = await _http.PostAsJsonAsync("api/Notas/guardar", dto);
                if (!response.IsSuccessStatusCode) _logger.LogWarning("Fallo al guardar nota.");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo de red al intentar guardar nota.");
                return false;
            }
        }

        public async Task<bool> CrearEvaluacionAsync(int idComision, EvaluacionDTO evaluacion)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/Notas/nueva-evaluacion/{idComision}", evaluacion);
                if (!response.IsSuccessStatusCode) _logger.LogWarning("Fallo al crear evaluación.");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar crear evaluación.");
                return false;
            }
        }

        public async Task<bool> EditarEvaluacionAsync(EvaluacionDTO evaluacion)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/Notas/editar-evaluacion", evaluacion);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar editar evaluación.");
                return false;
            }
        }

        public async Task<bool> EliminarEvaluacionAsync(int idEvaluacion)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/notas/evaluacion/{idEvaluacion}");
                if (!response.IsSuccessStatusCode) _logger.LogWarning("Fallo al eliminar evaluación.");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar evaluación.");
                return false;
            }
        }
    }
}