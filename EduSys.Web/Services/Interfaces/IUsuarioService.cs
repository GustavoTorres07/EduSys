using EduSys.Shared.DTOs;
using EduSys.Shared.Models; // Necesario si usas el modelo base para Crear

namespace EduSys.Web.Services.Interfaces
{
    public interface IUsuarioService
    {
        // Trae la lista completa de usuarios administrativos/sistema
        Task<List<UsuarioDTO>> GetAllAsync();

        // Trae los datos de un usuario específico por su ID
        Task<UsuarioDTO?> GetByIdAsync(int id);

        // Crea un nuevo usuario administrativo
        Task<Usuario?> CrearAsync(Usuario usuario);

        // Actualiza los datos de perfil (contacto) del usuario
        Task<bool> UpdateAsync(UsuarioDTO usuario);

        // 🚀 NUEVO: Actualiza la lista de roles asignados a un usuario
        Task<bool> UpdateRolesAsync(int idUsuario, List<int> rolesIds);
    }
}