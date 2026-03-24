using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 💡 Si quieres ser más estricto, podrías usar: [Authorize(Roles = "Alumno")]
    public class AlumnoPortalController : ControllerBase
    {
        private readonly IAlumnoPortalRepository _repo;

        public AlumnoPortalController(IAlumnoPortalRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("notificaciones")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<NotificacionDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NotificacionDTO>>> GetNotificaciones()
        {
            var idUsuario = ObtenerIdUsuarioLogueado();
            if (idUsuario == 0) return Unauthorized("Token inválido o usuario no identificado.");

            var notificaciones = await _repo.GetNotificacionesAsync(idUsuario);
            return Ok(notificaciones);
        }

        [HttpPut("notificaciones/leer/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            // Validamos que el usuario del token exista por seguridad
            if (ObtenerIdUsuarioLogueado() == 0) return Unauthorized();

            await _repo.MarcarNotificacionLeidaAsync(id);

            // 204 No Content es el estándar REST para PUTs exitosos sin body de respuesta
            return NoContent();
        }

        [HttpGet("mis-cursadas")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CursadaAlumnoDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<CursadaAlumnoDTO>>> GetMisCursadas()
        {
            var idUsuario = ObtenerIdUsuarioLogueado();
            if (idUsuario == 0) return Unauthorized("Token inválido o usuario no identificado.");

            var cursadas = await _repo.GetMisCursadasAsync(idUsuario);
            return Ok(cursadas);
        }

        [HttpGet("mis-asistencias")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AsistenciaMateriaDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<AsistenciaMateriaDTO>>> GetMisAsistencias()
        {
            try
            {
                // Usamos tu método auxiliar para mantener el código limpio
                var idUsuario = ObtenerIdUsuarioLogueado();
                if (idUsuario == 0) return Unauthorized("Token inválido o usuario no identificado.");

                // 🚀 AQUÍ ESTÁ LA CORRECCIÓN: Usamos _repo en lugar de _repository
                var asistencias = await _repo.GetMisAsistenciasAsync(idUsuario);

                return Ok(asistencias);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al procesar las asistencias: " + ex.Message);
            }
        }

        // Método auxiliar para sacar el ID del Token JWT
        private int ObtenerIdUsuarioLogueado()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }
    }
}