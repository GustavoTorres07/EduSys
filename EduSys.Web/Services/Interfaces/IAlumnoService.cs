using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IAlumnoService
    {
        // Obtiene el listado para la tabla (legajo, nombre, dni, carrera)
        Task<List<AlumnoListadoDTO>> GetAlumnosAsync();

        // Obtiene el detalle completo para el Modal de Legajo (incluyendo foto, datos, etc.)
        Task<AlumnoRequestDTO> GetAlumnoByIdAsync(int id);
        Task<bool> CrearAsync(AlumnoRequestDTO dto);
        Task<bool> EditarAsync(AlumnoRequestDTO dto);
        Task<AlumnoDTO?> GetByUsuarioIdAsync(int idUsuario);
    }
}