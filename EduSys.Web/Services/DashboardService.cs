using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly HttpClient _http;
        private readonly ILogger<DashboardService> _logger; // ✅ Inyectado para control de errores

        public DashboardService(HttpClient http, ILogger<DashboardService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<DashboardDTO> GetResumenAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<DashboardDTO>("api/dashboard");

                // Si la API devuelve nulo, instanciamos un objeto vacío para proteger la UI
                return response ?? new DashboardDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar cargar los datos del Dashboard.");

                // En caso de error de red, devolvemos un DTO vacío. 
                // La vista mostrará "0" en los indicadores en lugar de romperse.
                return new DashboardDTO();
            }
        }
    }
}