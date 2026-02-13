using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IAuthService
    {
        Task<SesionDTO> Login(LoginDTO loginDTO);
        Task Logout();

        Task<bool> CambiarClaveAsync(string nuevaClave);


        // 👇 NUEVO MÉTODO
        Task<bool> RecuperarClaveAsync(string email);
    }
}
