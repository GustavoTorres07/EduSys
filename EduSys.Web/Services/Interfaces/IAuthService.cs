using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IAuthService
    {
        Task<SesionDTO?> LoginAsync(LoginDTO loginDTO); // ✅ Añadido el sufijo Async y tipado nullable
        Task LogoutAsync(); // ✅ Añadido el sufijo Async
        Task<bool> CambiarClaveAsync(string nuevaClave);
        Task<bool> RecuperarClaveAsync(string email);
    }
}