using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class InscripcionService : IInscripcionService
    {
        private readonly HttpClient _http;
        private readonly ILogger<InscripcionService> _logger; // ✅ Agregado para trazabilidad

        public InscripcionService(HttpClient http, ILogger<InscripcionService> logger)
        {
            _http = http;
            _logger = logger;
        }

        // ==========================================
        // MÉTODOS DE ALUMNO
        // ==========================================

        public async Task<ResultadoInscripcionDTO> InscribirAlumnoAsync(InscripcionCursadaRequestDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/inscripciones/inscribir", dto);

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResultadoInscripcionDTO>();
                    return result ?? new ResultadoInscripcionDTO { Exito = false, Mensaje = "Error desconocido." };
                }

                _logger.LogWarning("Error HTTP {StatusCode} al inscribir alumno a cursada.", response.StatusCode);
                return new ResultadoInscripcionDTO { Exito = false, Mensaje = $"Error del servidor. Código: {response.StatusCode}." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Problema de red al procesar la inscripción a cursada.");
                return new ResultadoInscripcionDTO { Exito = false, Mensaje = "Problema de conexión con el servidor al procesar la inscripción." };
            }
        }

        public async Task<List<ComisionDTO>> GetOfertaParaAlumnoAsync(int idAlumno, int idPeriodo)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<ComisionDTO>>($"api/inscripciones/oferta/{idAlumno}?idPeriodo={idPeriodo}");
                return response ?? new List<ComisionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener oferta de materias para el alumno {IdAlumno}.", idAlumno);
                return new List<ComisionDTO>();
            }
        }

        public async Task<bool> CancelarInscripcionAsync(int idInscripcion)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/inscripciones/{idInscripcion}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar cancelar la inscripción {IdInscripcion}.", idInscripcion);
                return false;
            }
        }

        // ==========================================
        // MÉTODOS DE ADMIN / SECRETARÍA
        // ==========================================

        public async Task<ResultadoInscripcionDTO> InscribirAdminAsync(InscripcionManualDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/inscripciones/admin/inscribir", dto);

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var result = await response.Content.ReadFromJsonAsync<ResultadoInscripcionDTO>();
                    return result ?? new ResultadoInscripcionDTO { Exito = false, Mensaje = "Respuesta vacía del servidor." };
                }

                _logger.LogWarning("Error HTTP {StatusCode} en inscripción manual por administrador.", response.StatusCode);
                return new ResultadoInscripcionDTO { Exito = false, Mensaje = $"Error del servidor. Código: {response.StatusCode}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Problema de red al procesar la inscripción manual.");
                return new ResultadoInscripcionDTO { Exito = false, Mensaje = "Problema de conexión con el servidor." };
            }
        }

        public async Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesAlumnoAsync(int idAlumno, int idPeriodo)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<InscripcionCursadaListadoDTO>>($"api/inscripciones/alumno/{idAlumno}/periodo/{idPeriodo}");
                return response ?? new List<InscripcionCursadaListadoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inscripciones cursadas del alumno {IdAlumno} en periodo {IdPeriodo}.", idAlumno, idPeriodo);
                return new List<InscripcionCursadaListadoDTO>();
            }
        }

        public async Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesByAlumnoAsync(int idAlumno)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<InscripcionCursadaListadoDTO>>($"api/inscripciones/admin/alumno/{idAlumno}");
                return response ?? new List<InscripcionCursadaListadoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el historial de inscripciones cursadas para el alumno {IdAlumno}.", idAlumno);
                return new List<InscripcionCursadaListadoDTO>();
            }
        }
    }
}