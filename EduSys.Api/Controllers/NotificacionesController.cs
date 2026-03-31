using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Candado: Solo usuarios logueados
    public class NotificacionesController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificacionesController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Método privado para extraer el ID del usuario directamente del Token JWT
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return int.Parse(userIdClaim!.Value);
        }

        [HttpGet("mis-notificaciones")]
        public async Task<ActionResult<List<NotificacionDTO>>> GetMisNotificaciones()
        {
            var userId = GetUserId();
            var notificaciones = await _notificationService.GetNotificacionesByUsuarioAsync(userId);
            return Ok(notificaciones);
        }

        // ✅ CORREGIDO: Usamos HttpPut
        [HttpPut("marcar-leida/{id}")]
        public async Task<ActionResult> MarcarLeida(int id)
        {
            var userId = GetUserId();
            var result = await _notificationService.MarcarLeidaAsync(id, userId);

            if (result) return Ok();

            // Si retorna false, es porque la notificación no existe o intentó leer una de otro usuario (seguridad)
            return NotFound("Notificación no encontrada o no pertenece al usuario.");
        }

        // ✅ CORREGIDO: Usamos HttpPut
        [HttpPut("marcar-todas-leidas")]
        public async Task<ActionResult> MarcarTodasLeidas()
        {
            var userId = GetUserId();
            var result = await _notificationService.MarcarTodasLeidasAsync(userId);
            if (result) return Ok();
            return BadRequest("Error al actualizar notificaciones.");
        }
        [HttpPost("masiva")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒 Solo el alto mando puede usar esto
        public async Task<ActionResult> EnviarMasiva([FromBody] NotificacionMasivaDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Titulo) || string.IsNullOrWhiteSpace(request.Mensaje))
                return BadRequest("El título y el mensaje son obligatorios.");

            var result = await _notificationService.EnviarNotificacionMasivaAsync(request);
            if (result) return Ok();

            return BadRequest("No se encontró ningún usuario activo para ese filtro o hubo un error.");
        }

    }
}