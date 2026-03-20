using EduSys.Shared.DTOs;
using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IAlumnoRepository
    {
        // Obtener listado para la grilla (datos reducidos)
        Task<List<AlumnoListadoDTO>> GetAllAsync();

        // Obtener un alumno específico con TODOS sus datos (incluyendo URLs de archivos)
        Task<AlumnoRequestDTO?> GetByIdAsync(int id);

        // Crear Usuario + Alumno + Guardar URLs (Transaccional)
        Task<bool> CrearAsync(AlumnoRequestDTO dto);

        // Editar datos y URLs
        Task<bool> EditarAsync(AlumnoRequestDTO dto);

        // Baja lógica (Activo = false)
        Task<bool> EliminarAsync(int id);

        // Validación de existencia
        Task<bool> ExisteLegajoAsync(string legajo);

        Task<AlumnoDTO?> GetByUsuarioAsync(int idUsuario);

        Task<Alumno> CrearAsync(Alumno alumno);
    }
}