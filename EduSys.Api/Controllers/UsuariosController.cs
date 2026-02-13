using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization; // Importante
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Protegemos este controlador para que solo Admins puedan crear/ver usuarios
    [Authorize(Roles = "Administrador, Secretaria Academica")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuariosController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        // ELIMINADO: [HttpPost("login")] -> Ya está en AuthController

        // POST: api/usuarios (Crear usuarios administrativos manualmente)
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Usuario usuario)
        {
            // ... (Tu lógica de creación actual) ...
            string claveTextoPlano = usuario.ClaveHash;
            if (string.IsNullOrEmpty(claveTextoPlano)) return BadRequest("Falta clave.");

            try
            {
                var nuevo = await _usuarioRepository.RegistrarAsync(usuario, claveTextoPlano);
                return Ok(nuevo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}