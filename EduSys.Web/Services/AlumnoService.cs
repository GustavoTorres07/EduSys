using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class AlumnoService : IAlumnoService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AlumnoService> _logger; // ✅ Inyectado para depuración

        public AlumnoService(HttpClient http, ILogger<AlumnoService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<AlumnoListadoDTO>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<AlumnoListadoDTO>>("api/alumnos");
                return response ?? new List<AlumnoListadoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista general de alumnos.");
                return new List<AlumnoListadoDTO>();
            }
        }

        public async Task<AlumnoRequestDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<AlumnoRequestDTO>($"api/alumnos/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la información detallada del alumno con ID {Id}.", id);
                return null;
            }
        }

        public async Task<bool> CrearAsync(AlumnoRequestDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/alumnos", dto);

                if (!response.IsSuccessStatusCode)
                {
                    // 💡 Si falla, leemos el mensaje de la API ("El legajo ya existe", etc.)
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear alumno: {ErrorMessage}", errorMessage);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción crítica al intentar crear el alumno.");
                return false;
            }
        }

        public async Task<bool> EditarAsync(AlumnoRequestDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/alumnos", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al editar alumno {Id}: {ErrorMessage}", dto.IdAlumno, errorMessage);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción crítica al intentar editar el alumno con ID {Id}.", dto.IdAlumno);
                return false;
            }
        }

        public async Task<AlumnoDTO?> GetByUsuarioIdAsync(int idUsuario)
        {
            try
            {
                return await _http.GetFromJsonAsync<AlumnoDTO>($"api/alumnos/usuario/{idUsuario}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del alumno asociado al usuario {IdUsuario}.", idUsuario);
                return null;
            }
        }
    }
}