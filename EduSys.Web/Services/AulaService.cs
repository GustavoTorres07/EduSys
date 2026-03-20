using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class AulaService : IAulaService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AulaService> _logger; // ✅ Inyectado para depuración

        public AulaService(HttpClient http, ILogger<AulaService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<AulaDTO>> GetBySedeAsync(int idSede)
        {
            try
            {
                // ✅ CORRECCIÓN DE RUTA: Apuntamos al endpoint correcto del SedesController
                var response = await _http.GetFromJsonAsync<List<AulaDTO>>($"api/sedes/{idSede}/aulas");

                return response ?? new List<AulaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las aulas de la sede con ID {IdSede}.", idSede);
                return new List<AulaDTO>();
            }
        }
    }
}