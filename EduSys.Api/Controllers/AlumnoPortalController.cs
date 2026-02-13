using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Cualquier usuario logueado puede acceder (filtramos por ID interno)
    public class AlumnoPortalController : ControllerBase
    {
        private readonly IAlumnoPortalRepository _repo;

        public AlumnoPortalController(IAlumnoPortalRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("notificaciones")]
        public async Task<ActionResult<List<NotificacionDTO>>> GetNotificaciones()
        {
            var idUsuario = ObtenerIdUsuarioLogueado();
            return Ok(await _repo.GetNotificacionesAsync(idUsuario));
        }

        [HttpPost("notificaciones/leer/{id}")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            await _repo.MarcarNotificacionLeidaAsync(id);
            return Ok();
        }

        [HttpGet("mis-cursadas")]
        public async Task<ActionResult<List<CursadaAlumnoDTO>>> GetMisCursadas()
        {
            var idUsuario = ObtenerIdUsuarioLogueado();
            return Ok(await _repo.GetMisCursadasAsync(idUsuario));
        }

        // Método auxiliar para sacar el ID del Token JWT
        private int ObtenerIdUsuarioLogueado()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }
    }
}