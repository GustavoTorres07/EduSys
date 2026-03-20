using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class HistorialService : IHistorialService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HistorialService> _logger; // ✅ Agregado para trazabilidad

        public HistorialService(HttpClient http, ILogger<HistorialService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<HistoriaAcademicaDTO?> GetAvanceAsync(int idAlumno)
        {
            try
            {
                return await _http.GetFromJsonAsync<HistoriaAcademicaDTO>($"api/HistorialAcademico/avance/{idAlumno}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el avance académico del alumno con ID {IdAlumno}.", idAlumno);

                // Retornamos null para que la UI sepa que hubo un fallo y muestre un mensaje de error o alerta
                return null;
            }
        }

        public async Task<List<PeriodoHistorialDTO>> GetCronologicoAsync(int idAlumno)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<PeriodoHistorialDTO>>($"api/HistorialAcademico/cronologico/{idAlumno}");
                return response ?? new List<PeriodoHistorialDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el historial cronológico del alumno con ID {IdAlumno}.", idAlumno);

                // Retornamos lista vacía para no romper las tablas de la interfaz
                return new List<PeriodoHistorialDTO>();
            }
        }
    }
}