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
    public class DocentesController : ControllerBase
    {
        private readonly IDocenteRepository _docenteRepository;
        private readonly IEmailService _emailService;

        public DocentesController(IDocenteRepository docenteRepository, IEmailService emailService)
        {
            _docenteRepository = docenteRepository;
            _emailService = emailService;
        }

        // =================================================================================
        // SECCIÓN 1: MÉTODOS ADMINISTRATIVOS (ABM)
        // Solo para Administradores y Secretaría
        // =================================================================================

        [HttpGet]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DocenteListadoDTO>))]
        public async Task<ActionResult<List<DocenteListadoDTO>>> Get()
        {
            return Ok(await _docenteRepository.GetAllAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DocenteRequestDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocenteRequestDTO>> Get(int id)
        {
            var docente = await _docenteRepository.GetByIdAsync(id);
            if (docente == null) return NotFound(new { message = "Docente no encontrado." });
            return Ok(docente);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(DocenteRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _docenteRepository.CrearAsync(dto);

                try
                {
                    string asunto = "Bienvenido al Claustro Docente - EduSys";
                    string cuerpo = $@"
                        <div style='font-family: Arial, sans-serif; color: #333;'>
                            <h2 style='color: #456990;'>¡Bienvenido/a {dto.Nombre}!</h2>
                            <p>Has sido registrado exitosamente en el sistema de gestión académica <b>EduSys</b>.</p>
                            <p>Para ingresar, utiliza las siguientes credenciales temporales:</p>
                            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; border: 1px solid #ddd;'>
                                <p><b>Usuario:</b> {dto.Email}</p>
                                <p><b>Contraseña:</b> {dto.Dni}</p>
                            </div>
                            <br>
                            <p>⚠️ <i>Por seguridad, el sistema te pedirá cambiar esta contraseña al iniciar sesión por primera vez.</i></p>
                            <hr>
                            <p style='font-size: 12px; color: #777;'>Departamento de Sistemas - EduSys</p>
                        </div>";

                    await _emailService.SendEmailAsync(dto.Email, asunto, cuerpo);
                }
                catch (Exception emailEx)
                {
                    // Se creó en BD pero falló el correo. Devolvemos OK con advertencia.
                    return Ok(new { message = $"Docente creado, pero hubo un error enviando el correo de bienvenida: {emailEx.Message}" });
                }

                return Ok(new { message = "Docente creado y notificación enviada exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(DocenteRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resultado = await _docenteRepository.EditarAsync(dto);
            if (!resultado) return NotFound(new { message = "No se pudo editar el docente, es posible que no exista." });

            return Ok(new { message = "Docente actualizado correctamente." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var resultado = await _docenteRepository.EliminarAsync(id);
            if (!resultado) return NotFound(new { message = "No se encontró el docente a eliminar." });

            return NoContent();
        }

        // =================================================================================
        // SECCIÓN 2: MÉTODOS DEL DOCENTE (Dashboard y Gestión)
        // Accesibles por el rol Docente
        // =================================================================================

        [HttpGet("mis-comisiones")]
        [Authorize(Roles = "Docente, Administrador")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ComisionDocenteDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<ComisionDocenteDTO>>> GetMisComisiones()
        {
            var idUsuarioStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idUsuarioStr, out int idUsuario))
                return Unauthorized(new { message = "Token inválido o no se pudo identificar al usuario." });

            var comisiones = await _docenteRepository.GetMisComisionesAsync(idUsuario);
            return Ok(comisiones);
        }

        // Asegúrate de tener este using arriba en el controlador:
        // using System.Security.Claims;

        [HttpGet("mi-perfil")]
        [Authorize(Roles = "Docente")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DocenteRequestDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocenteRequestDTO>> GetMiPerfil()
        {
            // 1. Extraemos el Email (o el Name) directamente del Token JWT de quien hace la petición
            var emailUsuario = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(emailUsuario))
                return Unauthorized(new { message = "No se pudo identificar al usuario en el token de seguridad." });

            // 2. Buscamos el perfil en la base de datos
            var perfil = await _docenteRepository.GetMiPerfilAsync(emailUsuario);

            if (perfil == null)
                return NotFound(new { message = "No se encontró un perfil de docente vinculado a su usuario." });

            return Ok(perfil);
        }
    }
}