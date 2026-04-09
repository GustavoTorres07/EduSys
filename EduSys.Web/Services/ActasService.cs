using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class ActasService : IActasService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ActasService> _logger;

        public ActasService(HttpClient http, ILogger<ActasService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<bool> CerrarActaAsync(int idEvaluacion)
        {
            try
            {
                // 👈 Usamos la nueva ruta que configuramos en el controlador
                var response = await _http.PostAsync($"api/Actas/cerrar-acta/{idEvaluacion}", null);
                if (!response.IsSuccessStatusCode) _logger.LogWarning("Fallo al cerrar el acta de la evaluación.");
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
                var response = await _http.PostAsync($"api/Actas/reabrir-acta/{idEvaluacion}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conectividad al intentar reabrir el acta.");
                return false;
            }
        }

        public async Task<bool> CerrarCursadaAsync(CierreCursadaDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Actas/cerrar-cursada", dto);
                if (!response.IsSuccessStatusCode) _logger.LogWarning("Fallo al cerrar la cursada de la comisión.");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar cerrar la cursada.");
                return false;
            }
        }

        public async Task<bool> ToggleCierreIndividualAsync(int idInscripcion)
        {
            try
            {
                var res = await _http.PostAsync($"api/Actas/inscripcion/{idInscripcion}/toggle-cierre", null);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar cambiar el estado de cierre de la inscripción.");
                return false;
            }
        }

        public async Task<List<ActaResumenDTO>> GetActasPorAlumnoAsync(int idAlumno)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<ActaResumenDTO>>($"api/Actas/historial-alumno/{idAlumno}");
                return response ?? new List<ActaResumenDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las actas del alumno {IdAlumno}", idAlumno);
                return new List<ActaResumenDTO>();
            }
        }

        public async Task<bool> ReabrirComisionAsync(int idComision)
        {
            try
            {
                var res = await _http.PostAsync($"api/Actas/comision/{idComision}/reabrir", null);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar reabrir la comisión.");
                return false;
            }
        }
        public async Task<bool> CerrarCursadaAsync(int idComision)
        {
            var response = await _http.PostAsync($"api/Actas/cerrar-cursada/{idComision}", null);
            return response.IsSuccessStatusCode;
        }

    }
}