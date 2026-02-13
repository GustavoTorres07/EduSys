using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class AlumnoService : IAlumnoService
    {
        private readonly HttpClient _http;

        public AlumnoService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<AlumnoListadoDTO>> GetAlumnosAsync()
        {
            // Llama al endpoint GET api/alumnos
            var response = await _http.GetFromJsonAsync<List<AlumnoListadoDTO>>("api/alumnos");
            return response ?? new List<AlumnoListadoDTO>();
        }

        public async Task<AlumnoRequestDTO> GetAlumnoByIdAsync(int id)
        {
            // Llama al endpoint GET api/alumnos/{id}
            var response = await _http.GetFromJsonAsync<AlumnoRequestDTO>($"api/alumnos/{id}");

            if (response == null)
                throw new Exception("No se pudo obtener la información del alumno.");

            return response;
        }

        public async Task<bool> CrearAsync(AlumnoRequestDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/alumnos", dto);
            return response.IsSuccessStatusCode;
        }

        // ✅ IMPLEMENTACIÓN DE EDITAR
        public async Task<bool> EditarAsync(AlumnoRequestDTO dto)
        {
            // Nota: Asumimos que el endpoint es PUT api/alumnos
            var response = await _http.PutAsJsonAsync("api/alumnos", dto);
            return response.IsSuccessStatusCode;
        }
        public async Task<AlumnoDTO?> GetByUsuarioIdAsync(int idUsuario)
        {
            try
            {
                // Opción A: Si creaste el endpoint en backend (Recomendado)
                // return await _http.GetFromJsonAsync<AlumnoDTO>($"api/alumnos/usuario/{idUsuario}");

                // Opción B: Filtrado en cliente (Rápido para salir del paso)
                // Nota: Esto requiere que tengas un endpoint que devuelva DTOs completos, 
                // o mapear de AlumnoListadoDTO si tiene IdUsuario.

                // Vamos a asumir que necesitas crear el endpoint en el Backend para hacerlo bien.
                // Mientras tanto, si usas la Opción B, asegúrate que el DTO de listado tenga IdUsuario.

                // MÉTODO RECOMENDADO: Llamar a un endpoint específico que vamos a crear ahora.
                return await _http.GetFromJsonAsync<AlumnoDTO>($"api/alumnos/usuario/{idUsuario}");
            }
            catch
            {
                return null;
            }
        }
    }
}