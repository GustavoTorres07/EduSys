using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IUsuarioService
    {
        // Trae los datos de un usuario por su ID
        Task<UsuarioDTO?> GetByIdAsync(int id);

        // Actualiza los datos del usuario
        Task<bool> UpdateAsync(UsuarioDTO usuario);

        // (Si en el futuro necesitas listar todos los usuarios o eliminarlos, los agregas aquí)
        // Task<List<UsuarioDTO>> GetAllAsync();
        // Task<bool> DeleteAsync(int id);
    }
}