using EduSys.Api.Repositories.Interfaces;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmailService _emailService;

        // ✅ INYECCIÓN LIMPIA: Adiós DbContext
        public AuthController(IUsuarioRepository usuarioRepository, IEmailService emailService)
        {
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SesionDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sesion = await _usuarioRepository.LoginAsync(login);

            if (sesion == null)
                return Unauthorized(new { message = "Credenciales inválidas." });

            return Ok(sesion);
        }

        // POST: api/auth/cambiar-clave
        [HttpPost("cambiar-clave")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CambiarClave([FromBody] CambioClaveDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Token inválido." });

            try
            {
                // Encriptamos en el controller o service layer, no en la DB
                string hash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaClave);

                bool exito = await _usuarioRepository.CambiarClaveDesdePerfilAsync(userId, hash);

                if (!exito) return NotFound(new { message = "Usuario no encontrado." });

                return Ok(new { message = "Contraseña actualizada." });
            }
            catch (Exception ex)
            {
                // Loguear excepción internamente (ex)
                return BadRequest(new { message = "Ocurrió un error al cambiar la contraseña." });
            }
        }

        // POST: api/auth/recuperar
        [HttpPost("recuperar")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RecuperarClave([FromBody] RecuperarClaveRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool existe = await _usuarioRepository.ExisteEmailAsync(dto.Email);

            // Prevención de enumeración de usuarios (Seguridad OWASP)
            if (!existe) return Ok(new { message = "Si el correo está registrado, recibirás instrucciones." });

            try
            {
                string claveTemporal = Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
                string hashTemporal = BCrypt.Net.BCrypt.HashPassword(claveTemporal);

                await _usuarioRepository.RestablecerClaveAsync(dto.Email, hashTemporal);

                string cuerpo = $@"
                    <div style='font-family: Arial, sans-serif; color: #333;'>
                        <h2>Recuperación de Contraseña - EduSys</h2>
                        <p>Hemos recibido una solicitud para restablecer tu contraseña.</p>
                        <p>Tu nueva contraseña temporal es:</p>
                        <h3 style='color: #456990; background: #f0f0f0; padding: 10px; display: inline-block;'>{claveTemporal}</h3>
                        <p>Por favor ingresa con esta clave. <b>El sistema te pedirá cambiarla inmediatamente.</b></p>
                    </div>";

                await _emailService.SendEmailAsync(dto.Email, "Recuperación de Acceso", cuerpo);

                return Ok(new { message = "Si el correo está registrado, recibirás instrucciones." });
            }
            catch (Exception)
            {
                // Evitamos dar detalles del servidor de correo al cliente
                return BadRequest(new { message = "Ocurrió un problema procesando la solicitud. Intente más tarde." });
            }
        }
    }
}