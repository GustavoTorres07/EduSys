using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IAlumnoService
    {
        // Unificamos GetAlumnosAsync y GetAllAsync en uno solo
        Task<List<AlumnoListadoDTO>> GetAllAsync();

        // Obtiene el detalle completo para el Modal de Legajo
        Task<AlumnoRequestDTO?> GetMiPerfilAsync();

        Task<AlumnoRequestDTO?> GetByIdAsync(int id);
        Task<bool> CrearAsync(AlumnoRequestDTO dto);

        Task<bool> EditarAsync(AlumnoRequestDTO dto);

        Task<AlumnoDTO?> GetByUsuarioIdAsync(int idUsuario);
    }
}