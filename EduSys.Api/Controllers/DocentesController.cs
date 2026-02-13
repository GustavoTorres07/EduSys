using EduSys.Api.Repositories.Interfaces;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // Necesario para leer el Token

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
        public async Task<ActionResult<List<DocenteListadoDTO>>> Get()
        {
            return Ok(await _docenteRepository.GetAllAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult<DocenteRequestDTO>> Get(int id)
        {
            var docente = await _docenteRepository.GetByIdAsync(id);
            if (docente == null) return NotFound("Docente no encontrado");
            return Ok(docente);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult> Post(DocenteRequestDTO dto)
        {
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
                    return Ok(new { message = $"Docente creado, pero hubo un error enviando el correo: {emailEx.Message}" });
                }

                return Ok(new { message = "Docente creado y notificación enviada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult> Put(DocenteRequestDTO dto)
        {
            var resultado = await _docenteRepository.EditarAsync(dto);
            if (!resultado) return NotFound("No se pudo editar el docente");
            return Ok(new { message = "Docente actualizado" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult> Delete(int id)
        {
            var resultado = await _docenteRepository.EliminarAsync(id);
            if (!resultado) return NotFound("No se encontró el docente");
            return Ok(new { message = "Docente dado de baja" });
        }

        // =================================================================================
        // SECCIÓN 2: MÉTODOS DEL DOCENTE (Dashboard y Gestión)
        // Accesibles por el rol Docente
        // =================================================================================

        [HttpGet("mis-comisiones")]
        [Authorize(Roles = "Docente, Administrador")] // ✅ Permitimos al docente ver sus datos
        public async Task<ActionResult<List<ComisionDocenteDTO>>> GetMisComisiones()
        {
            // Obtenemos el ID del usuario directamente del Token JWT
            var idUsuarioStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idUsuarioStr, out int idUsuario))
                return BadRequest("No se pudo identificar al usuario.");

            var comisiones = await _docenteRepository.GetMisComisionesAsync(idUsuario);
            return Ok(comisiones);
        }
    }
}