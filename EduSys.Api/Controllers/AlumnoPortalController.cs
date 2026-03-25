using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔓 Candado Base: Exige que el usuario esté logueado (Cualquier rol)
    [Authorize]
    public class AlumnoPortalController : ControllerBase
    {
        private readonly IAlumnoPortalRepository _repo;

        public AlumnoPortalController(IAlumnoPortalRepository repo)
        {
            _repo = repo;
        }

        // ========================================================
        // NOTIFICACIONES (Disponible para TODOS los roles)
        // ========================================================
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
            if (ObtenerIdUsuarioLogueado() == 0) return Unauthorized();

            await _repo.MarcarNotificacionLeidaAsync(id);
            return NoContent();
        }

        // ========================================================
        // PORTAL ACADÉMICO (Exclusivo para ALUMNOS)
        // ========================================================

        [HttpGet("mis-cursadas")]
        // 🔒 CANDADO REAL: Solo usuarios con el rol "Alumno" pueden entrar aquí
        [Authorize(Roles = "Alumno")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CursadaAlumnoDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] // 403 si entra alguien que no es alumno
        public async Task<ActionResult<List<CursadaAlumnoDTO>>> GetMisCursadas()
        {
            var idUsuario = ObtenerIdUsuarioLogueado();
            if (idUsuario == 0) return Unauthorized("Token inválido o usuario no identificado.");

            var cursadas = await _repo.GetMisCursadasAsync(idUsuario);
            return Ok(cursadas);
        }

        [HttpGet("mis-asistencias")]
        // 🔒 CANDADO REAL: Solo usuarios con el rol "Alumno" pueden entrar aquí
        [Authorize(Roles = "Alumno")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AsistenciaMateriaDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<AsistenciaMateriaDTO>>> GetMisAsistencias()
        {
            try
            {
                var idUsuario = ObtenerIdUsuarioLogueado();
                if (idUsuario == 0) return Unauthorized("Token inválido o usuario no identificado.");

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