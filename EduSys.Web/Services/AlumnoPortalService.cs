using System.Net.Http.Json;
using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EduSys.Web.Services
{
    public class AlumnoPortalService : IAlumnoPortalService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AlumnoPortalService> _logger;

        public AlumnoPortalService(HttpClient http, ILogger<AlumnoPortalService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<NotificacionDTO>> GetNotificacionesAsync()
        {
            try
            {
                var resultado = await _http.GetFromJsonAsync<List<NotificacionDTO>>("api/AlumnoPortal/notificaciones");
                return resultado ?? new List<NotificacionDTO>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Acceso no autorizado (401) al buscar notificaciones. Posible token expirado.");
                return new List<NotificacionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las notificaciones del alumno.");
                return new List<NotificacionDTO>();
            }
        }

        public async Task<AlumnoDTO> GetPerfilAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<AlumnoDTO>("api/alumnoportal/perfil");
                return response ?? new AlumnoDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del alumno.");
                return new AlumnoDTO();
            }
        }

        public async Task MarcarLeidaAsync(int id)
        {
            try
            {
                var response = await _http.PostAsync($"api/AlumnoPortal/notificaciones/leer/{id}", null);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar marcar la notificación {Id} como leída.", id);
            }
        }

        public async Task<List<CursadaAlumnoDTO>> GetMisCursadasAsync()
        {
            try
            {
                var resultado = await _http.GetFromJsonAsync<List<CursadaAlumnoDTO>>("api/AlumnoPortal/mis-cursadas");
                return resultado ?? new List<CursadaAlumnoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de cursadas del alumno.");
                return new List<CursadaAlumnoDTO>();
            }
        }

        public async Task<List<AsistenciaMateriaDTO>> GetMisAsistenciasAsync()
        {
            try
            {
                var resultado = await _http.GetFromJsonAsync<List<AsistenciaMateriaDTO>>("api/AlumnoPortal/mis-asistencias");
                return resultado ?? new List<AsistenciaMateriaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de asistencias del alumno.");
                return new List<AsistenciaMateriaDTO>();
            }
        }

        // 🚀 NUEVO MÉTODO PARA EXÁMENES FINALES
        public async Task<List<InscripcionFinalDTO>> GetMisInscripcionesFinalesAsync()
        {
            try
            {
                // Asumo que esta es la ruta en tu API. ¡Asegúrate de que coincida con tu Controlador!
                var resultado = await _http.GetFromJsonAsync<List<InscripcionFinalDTO>>("api/AlumnoPortal/mis-finales");
                return resultado ?? new List<InscripcionFinalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de inscripciones a finales del alumno.");
                return new List<InscripcionFinalDTO>();
            }
        }
    }
}