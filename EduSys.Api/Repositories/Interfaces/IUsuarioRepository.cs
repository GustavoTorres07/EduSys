using EduSys.Shared.DTOs;
using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<SesionDTO> LoginAsync(LoginDTO login);
        Task<Usuario> RegistrarAsync(Usuario usuario, string claveTextoPlano);
        Task<Usuario> CrearAsync(Usuario usuario);
        Task<bool> ExisteEmailAsync(string email);
        Task<bool> RestablecerClaveAsync(string email, string nuevaClaveHash);
        Task<bool> CambiarClaveDesdePerfilAsync(int idUsuario, string nuevaClaveHash);
        Task<Usuario?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(Usuario usuario);

        // 👇 NUEVOS MÉTODOS AGREGADOS:
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<bool> ActualizarRolesAsync(int idUsuario, List<int> rolesIds);
    }
}