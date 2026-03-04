using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class NotasService : INotasService
    {
        private readonly HttpClient _http;

        public NotasService(HttpClient http)
        {
            _http = http;
        }

        public async Task<PlanillaNotasDTO?> GetPlanillaAsync(int idComision)
        {
            return await _http.GetFromJsonAsync<PlanillaNotasDTO>($"api/Notas/planilla/{idComision}");
        }

        public async Task<bool> GuardarNotaAsync(int idInscripcion, int idEvaluacion, decimal? valor)
        {
            var dto = new GuardarNotaDTO
            {
                IdInscripcion = idInscripcion,
                IdEvaluacion = idEvaluacion,
                Valor = valor // Ahora acepta null
            };

            var response = await _http.PostAsJsonAsync("api/Notas/guardar", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CrearEvaluacionAsync(int idComision, EvaluacionDTO evaluacion)
        {
            var response = await _http.PostAsJsonAsync($"api/Notas/nueva-evaluacion/{idComision}", evaluacion);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EditarEvaluacionAsync(EvaluacionDTO evaluacion)
        {
            var response = await _http.PutAsJsonAsync("api/Notas/editar-evaluacion", evaluacion);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CerrarActaAsync(CierreActaDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/Notas/cerrar-acta", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ReabrirActaAsync(int idEvaluacion)
        {
            var response = await _http.PostAsync($"api/Notas/reabrir-acta/{idEvaluacion}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CerrarCursadaAsync(CierreCursadaDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/notas/cerrar-cursada", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarEvaluacionAsync(int idEvaluacion)
        {
            var res = await _http.DeleteAsync($"api/notas/evaluacion/{idEvaluacion}");
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> ToggleCierreIndividualAsync(int idInscripcion)
        {
            var res = await _http.PostAsync($"api/notas/inscripcion/{idInscripcion}/toggle-cierre", null);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> ReabrirComisionAsync(int idComision)
        {
            var res = await _http.PostAsync($"api/notas/comision/{idComision}/reabrir", null);
            return res.IsSuccessStatusCode;
        }
    }
}