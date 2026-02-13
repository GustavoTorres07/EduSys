using EduSys.Shared.DTOs;
using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        // ... (tus métodos existentes LoginAsync y RegistrarAsync) ...
        Task<SesionDTO> LoginAsync(LoginDTO login);
        Task<Usuario> RegistrarAsync(Usuario usuario, string claveTextoPlano);

        // 👇 AGREGAR ESTOS DOS:
        Task<Usuario> CrearAsync(Usuario usuario);
        Task<bool> ExisteEmailAsync(string email);
        Task<bool> RestablecerClaveAsync(string email, string nuevaClaveHash);
    }
}
