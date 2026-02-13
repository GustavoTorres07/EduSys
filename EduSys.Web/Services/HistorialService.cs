using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace EduSys.Web.Services
{
    public class HistorialService : IHistorialService
    {
        private readonly HttpClient _http;

        public HistorialService(HttpClient http)
        {
            _http = http;
        }

        public async Task<HistoriaAcademicaDTO> GetAvanceAsync(int idAlumno)
        {
            return await _http.GetFromJsonAsync<HistoriaAcademicaDTO>($"api/HistorialAcademico/avance/{idAlumno}");
        }

        public async Task<List<PeriodoHistorialDTO>> GetCronologicoAsync(int idAlumno)
        {
            return await _http.GetFromJsonAsync<List<PeriodoHistorialDTO>>($"api/HistorialAcademico/cronologico/{idAlumno}")
                   ?? new List<PeriodoHistorialDTO>();
        }
    }
}