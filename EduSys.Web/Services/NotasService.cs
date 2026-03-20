using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class NotasService : INotasService
    {
        private readonly HttpClient _http;
        private readonly ILogger<NotasService> _logger; // ✅ Inyectado para depuración profesional

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
                var dto = new GuardarNotaDTO
                {
                    IdInscripcion = idInscripcion,
                    IdEvaluacion = idEvaluacion,
                    Valor = valor
                };

                var response = await _http.PostAsJsonAsync("api/Notas/guardar", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al guardar nota (Insc: {Insc}, Eval: {Eval}): {ErrorMsg}", idInscripcion, idEvaluacion, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo de red al intentar guardar la nota del alumno.");
                return false;
            }
        }

        public async Task<bool> CrearEvaluacionAsync(int idComision, EvaluacionDTO evaluacion)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/Notas/nueva-evaluacion/{idComision}", evaluacion);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear la evaluación para la comisión {IdComision}: {ErrorMsg}", idComision, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar crear una evaluación en la comisión {IdComision}.", idComision);
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
                _logger.LogError(ex, "Error crítico al intentar editar la evaluación {IdEvaluacion}.", evaluacion.IdEvaluacion);
                return false;
            }
        }

        public async Task<bool> CerrarActaAsync(CierreActaDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Notas/cerrar-acta", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al cerrar el acta de la evaluación {IdEvaluacion}: {ErrorMsg}", dto.IdEvaluacion, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conectividad al intentar cerrar el acta.");
                return false;
            }
        }

        public async Task<bool> ReabrirActaAsync(int idEvaluacion)
        {
            try
            {
                var response = await _http.PostAsync($"api/Notas/reabrir-acta/{idEvaluacion}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conectividad al intentar reabrir el acta de la evaluación {IdEvaluacion}.", idEvaluacion);
                return false;
            }
        }

        public async Task<bool> CerrarCursadaAsync(CierreCursadaDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/notas/cerrar-cursada", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al cerrar la cursada de la comisión {IdComision}: {ErrorMsg}", dto.IdComision, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar cerrar la cursada de la comisión {IdComision}.", dto.IdComision);
                return false;
            }
        }

        public async Task<bool> EliminarEvaluacionAsync(int idEvaluacion)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/notas/evaluacion/{idEvaluacion}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al eliminar la evaluación {IdEvaluacion}: {ErrorMsg}", idEvaluacion, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // ✅ Sustituimos el Console.WriteLine por ILogger
                _logger.LogError(ex, "Error al eliminar la evaluación {IdEvaluacion}.", idEvaluacion);
                return false;
            }
        }

        public async Task<bool> ToggleCierreIndividualAsync(int idInscripcion)
        {
            try
            {
                var res = await _http.PostAsync($"api/notas/inscripcion/{idInscripcion}/toggle-cierre", null);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar cambiar el estado de cierre de la inscripción {IdInscripcion}.", idInscripcion);
                return false;
            }
        }

        public async Task<bool> ReabrirComisionAsync(int idComision)
        {
            try
            {
                var res = await _http.PostAsync($"api/notas/comision/{idComision}/reabrir", null);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar reabrir la comisión {IdComision}.", idComision);
                return false;
            }
        }
    }
}