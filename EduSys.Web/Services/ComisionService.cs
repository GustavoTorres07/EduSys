using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class ComisionService : IComisionService
    {
        private readonly HttpClient _http;
        public ComisionService(HttpClient http) { _http = http; }

        public async Task<List<ComisionDTO>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<ComisionDTO>>("api/comisiones") ?? new List<ComisionDTO>();

        public async Task<List<ComisionDTO>> GetByPeriodoAsync(int idPeriodo)
            => await _http.GetFromJsonAsync<List<ComisionDTO>>($"api/comisiones/periodo/{idPeriodo}") ?? new List<ComisionDTO>();

        public async Task<ComisionDTO> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<ComisionDTO>($"api/comisiones/{id}");

        public async Task<bool> CreateAsync(ComisionDTO dto)
            => (await _http.PostAsJsonAsync("api/comisiones", dto)).IsSuccessStatusCode;

        public async Task<bool> UpdateAsync(ComisionDTO dto)
            => (await _http.PutAsJsonAsync("api/comisiones", dto)).IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int id)
            => (await _http.DeleteAsync($"api/comisiones/{id}")).IsSuccessStatusCode;

        public async Task<List<ComisionDTO>> GetByPeriodoAndCarreraAsync(int idPeriodo, int idCarrera, int? idAlumno = null)
        {
            string url = $"api/comisiones/periodo/{idPeriodo}/carrera/{idCarrera}";

            // Si nos pasan el ID del alumno, lo agregamos como QueryString para que el backend valide correlativas
            if (idAlumno.HasValue)
            {
                url += $"?idAlumno={idAlumno.Value}";
            }

            return await _http.GetFromJsonAsync<List<ComisionDTO>>(url) ?? new List<ComisionDTO>();
        }

        // ✅ IMPLEMENTACIÓN DE MÉTODOS DOCENTES

        public async Task<List<DocenteComisionListadoDTO>> GetDocentesPorComisionAsync(int idComision)
        {
            var response = await _http.GetAsync($"api/comisiones/{idComision}/docentes");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<DocenteComisionListadoDTO>>()
                       ?? new List<DocenteComisionListadoDTO>();
            }
            return new List<DocenteComisionListadoDTO>();
        }

        public async Task<ResultadoOperacionDTO> AsignarDocenteAsync(DocenteComisionRequestDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/comisiones/asignar-docente", dto);

            // Leemos el DTO directamente. El controlador se encarga de llenarlo bien.
            var resultado = await response.Content.ReadFromJsonAsync<ResultadoOperacionDTO>();

            return resultado ?? new ResultadoOperacionDTO { Exito = false, Mensaje = "Error de conexión o respuesta vacía." };
        }


        public async Task<List<ComisionDTO>> GetComisionesPorSedeAsync(int idSede)
        {
            // Llamamos al endpoint del backend que filtra por sede
            // Nota: Asegúrate de que la ruta coincida con el controlador (ver paso 3)
            var response = await _http.GetFromJsonAsync<List<ComisionDTO>>($"api/comisiones/sede/{idSede}");
            return response ?? new List<ComisionDTO>();
        }
        public async Task<bool> DesasignarDocenteAsync(int idAsignacion)
        {
            var response = await _http.DeleteAsync($"api/comisiones/docentes/{idAsignacion}");
            return response.IsSuccessStatusCode;
        }
    }
}