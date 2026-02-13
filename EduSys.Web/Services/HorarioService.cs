using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic; // Agregado por si falta
using System.Threading.Tasks;     // Agregado por si falta
using System;                     // Agregado por la Exception

namespace EduSys.Web.Services
{
    public class HorarioService : IHorarioService
    {
        private readonly HttpClient _http; // <--- SE LLAMA _http

        public HorarioService(HttpClient http) { _http = http; }

        public async Task<List<HorarioComisionDTO>> GetByComisionAsync(int idComision)
            => await _http.GetFromJsonAsync<List<HorarioComisionDTO>>($"api/horarios/comision/{idComision}") ?? new List<HorarioComisionDTO>();

        public async Task<bool> CreateAsync(HorarioComisionDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/horarios", dto);
            // Si hay conflicto (Aula ocupada), el backend devuelve 409 Conflict
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error); // Lanzamos el error para mostrarlo en la alerta
            }
            return true;
        }

        public async Task<byte[]> DescargarPdfAsync(int idPeriodo, int idCarrera, int idSede)
        {
            var url = $"api/reportes/horario-descargar?idPeriodo={idPeriodo}&idCarrera={idCarrera}&idSede={idSede}";
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error); // Lanzamos el error para mostrarlo en la alerta
            }
        }

        public async Task<bool> DeleteAsync(int id)
            => (await _http.DeleteAsync($"api/horarios/{id}")).IsSuccessStatusCode;

        public async Task<List<HorarioVisualizacionDTO>> GetVisualizacionAsync(int idPeriodo, int idCarrera, int idSede)
        {
            return await _http.GetFromJsonAsync<List<HorarioVisualizacionDTO>>($"api/horarios/visualizacion/periodo/{idPeriodo}/carrera/{idCarrera}/sede/{idSede}")
                    ?? new List<HorarioVisualizacionDTO>();
        }

        public async Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno)
        {
            // CORREGIDO: Usamos _http en lugar de _httpClient
            var response = await _http.GetFromJsonAsync<List<HorarioVisualizacionDTO>>(
                $"api/reportes/horarios-alumno-cursando?idPeriodo={idPeriodo}&idAlumno={idAlumno}");

            return response ?? new List<HorarioVisualizacionDTO>();
        }
    }
}