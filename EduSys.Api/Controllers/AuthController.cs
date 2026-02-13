using EduSys.Api.Repositories.Interfaces;
using EduSys.Api.Data;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using EduSys.Api.Services; // ✅ Asegúrate de tener este using
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EduSys.Api.Services.Interfaces;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly EduSysDbContext _context;
        private readonly IEmailService _emailService; // ✅ 1. Declarar el campo

        // ✅ 2. Inyectar en el constructor
        public AuthController(IUsuarioRepository usuarioRepository,
                              EduSysDbContext context,
                              IEmailService emailService)
        {
            _usuarioRepository = usuarioRepository;
            _context = context;
            _emailService = emailService; // ✅ 3. Asignar
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var sesion = await _usuarioRepository.LoginAsync(login);
            if (sesion == null) return Unauthorized("Credenciales inválidas.");
            return Ok(sesion);
        }

        // POST: api/auth/cambiar-clave
        [HttpPost("cambiar-clave")]
        [Authorize]
        public async Task<IActionResult> CambiarClave([FromBody] CambioClaveDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
                int userId = int.Parse(userIdClaim);

                var usuario = await _context.Usuarios.FindAsync(userId);
                if (usuario == null) return NotFound();

                usuario.ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaClave);
                usuario.DebeCambiarPass = false;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Contraseña actualizada." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/auth/recuperar
        [HttpPost("recuperar")]
        public async Task<IActionResult> RecuperarClave([FromBody] RecuperarClaveRequestDTO dto)
        {
            // 1. Verificar si el usuario existe
            bool existe = await _usuarioRepository.ExisteEmailAsync(dto.Email);

            // Por seguridad, respondemos OK aunque no exista para no filtrar datos
            if (!existe) return Ok(new { message = "Solicitud procesada." });

            try
            {
                // 2. Generar clave temporal
                string claveTemporal = Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
                string hashTemporal = BCrypt.Net.BCrypt.HashPassword(claveTemporal);

                // 3. Guardar en BD
                await _usuarioRepository.RestablecerClaveAsync(dto.Email, hashTemporal);

                // 4. Enviar Email
                string cuerpo = $@"
                    <div style='font-family: Arial, sans-serif; color: #333;'>
                        <h2>Recuperación de Contraseña - EduSys</h2>
                        <p>Hemos recibido una solicitud para restablecer tu contraseña.</p>
                        <p>Tu nueva contraseña temporal es:</p>
                        <h3 style='color: #456990; background: #f0f0f0; padding: 10px; display: inline-block;'>{claveTemporal}</h3>
                        <p>Por favor ingresa con esta clave. <b>El sistema te pedirá cambiarla inmediatamente.</b></p>
                    </div>";

                await _emailService.SendEmailAsync(dto.Email, "Recuperación de Acceso", cuerpo);

                return Ok(new { message = "Correo enviado." });
            }
            catch (Exception)
            {
                return BadRequest("No se pudo enviar el correo.");
            }
        }
    }
}