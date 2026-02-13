using EduSys.Shared.Models;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class AulaService : IAulaService
    {
        private readonly HttpClient _http;
        public AulaService(HttpClient http) { _http = http; }

        public async Task<List<Aula>> GetBySedeAsync(int idSede)
            => await _http.GetFromJsonAsync<List<Aula>>($"api/aulas/sede/{idSede}") ?? new List<Aula>();
    }
}