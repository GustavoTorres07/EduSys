using System.Net.Http.Json;
using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging; // ✅ Trazabilidad

namespace EduSys.Web.Services
{
    public class InscripcionFinalService : IInscripcionFinalService
    {
        private readonly HttpClient _http;
        private readonly ILogger<InscripcionFinalService> _logger; // ✅ Inyectamos el logger

        public InscripcionFinalService(HttpClient http, ILogger<InscripcionFinalService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<MesaFinalOfertaDTO>> GetOfertaAsync(int idAlumno, int idPeriodo)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<MesaFinalOfertaDTO>>($"api/inscripcionesfinales/oferta/{idAlumno}?idPeriodo={idPeriodo}")
                       ?? new List<MesaFinalOfertaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar obtener la oferta de finales para el alumno {IdAlumno}.", idAlumno);
                return new List<MesaFinalOfertaDTO>();
            }
        }

        public async Task<List<MesaFinalOfertaDTO>> GetMisInscripcionesAsync(int idAlumno, int idPeriodo)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<MesaFinalOfertaDTO>>($"api/inscripcionesfinales/mis-inscripciones/{idAlumno}?idPeriodo={idPeriodo}")
                       ?? new List<MesaFinalOfertaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar obtener las inscripciones del alumno {IdAlumno}.", idAlumno);
                return new List<MesaFinalOfertaDTO>();
            }
        }

        public async Task<ResultadoOperacionDTO> InscribirAsync(InscripcionFinalRequestDTO dto)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/inscripcionesfinales/inscribir", dto);

                // Si la inscripción es exitosa o si hay un error de validación de negocio controlado (BadRequest)
                if (res.IsSuccessStatusCode || res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return await res.Content.ReadFromJsonAsync<ResultadoOperacionDTO>()
                           ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Respuesta vacía del servidor." };
                }

                _logger.LogWarning("Error HTTP {StatusCode} al intentar inscribir al alumno en el final.", res.StatusCode);
                return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error del servidor: {res.StatusCode}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción crítica al intentar inscribir al alumno.");
                return new ResultadoOperacionDTO { Exito = false, Mensaje = "Error de conexión con el servidor. Por favor, reintente en unos minutos." };
            }
        }

        public async Task<ResultadoOperacionDTO> CancelarInscripcionAsync(int idInscripcion, int idAlumno)
        {
            try
            {
                var res = await _http.DeleteAsync($"api/inscripcionesfinales/cancelar/{idInscripcion}?idAlumno={idAlumno}");

                if (res.IsSuccessStatusCode || res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return await res.Content.ReadFromJsonAsync<ResultadoOperacionDTO>()
                           ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Respuesta vacía del servidor." };
                }

                _logger.LogWarning("Error HTTP {StatusCode} al intentar cancelar la inscripción {IdInscripcion}.", res.StatusCode, idInscripcion);
                return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error del servidor: {res.StatusCode}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción crítica al intentar cancelar la inscripción {IdInscripcion}.", idInscripcion);
                return new ResultadoOperacionDTO { Exito = false, Mensaje = "Error de conexión con el servidor. Por favor, reintente en unos minutos." };
            }
        }
    }
}