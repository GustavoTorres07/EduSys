using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduSys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SoporteController : ControllerBase
    {
        private readonly ISoporteRepository _soporteRepo;

        public SoporteController(ISoporteRepository soporteRepo)
        {
            _soporteRepo = soporteRepo;
        }

        // =========================================================
        // 1. ENDPOINT PÚBLICO (Accesible desde el Login)
        // =========================================================
        [AllowAnonymous]
        [HttpPost("publico")]
        public async Task<IActionResult> CrearTicketPublico([FromBody] TicketPublicoRequestDTO request)
        {
            try
            {
                // Buscamos si existe la identificación (DNI o Correo)
                var idUsuario = await _soporteRepo.ObtenerIdUsuarioPorIdentificacionAsync(request.Identificacion);

                if (idUsuario == null)
                {
                    return BadRequest(new { message = "No se encontró ningún usuario activo con ese DNI o Correo. Por favor verifique sus datos o contáctese con la institución telefónicamente." });
                }

                var ticket = await _soporteRepo.CrearTicketAsync(idUsuario.Value, request.Categoria, request.Asunto, request.Mensaje);
                return Ok(new { message = "Ticket creado exitosamente.", ticket });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el ticket.", detalle = ex.Message });
            }
        }

        // =========================================================
        // 2. ENDPOINTS PARA USUARIOS LOGUEADOS (Alumnos/Docentes)
        // =========================================================
        [Authorize]
        [HttpPost("interno")]
        public async Task<IActionResult> CrearTicketInterno([FromBody] TicketInternoRequestDTO request)
        {
            var userId = ObtenerIdUsuarioActual();
            var ticket = await _soporteRepo.CrearTicketAsync(userId, request.Categoria, request.Asunto, request.Mensaje);
            return Ok(new { message = "Ticket creado exitosamente.", ticket });
        }

        [Authorize]
        [HttpGet("mis-tickets")]
        public async Task<IActionResult> GetMisTickets()
        {
            var userId = ObtenerIdUsuarioActual();
            var tickets = await _soporteRepo.GetTicketsPorUsuarioAsync(userId);
            return Ok(tickets);
        }

        [Authorize]
        [HttpGet("{idTicket}")]
        public async Task<IActionResult> GetDetalleTicket(int idTicket)
        {
            var userId = ObtenerIdUsuarioActual();

            // Seguridad: Verificamos que el ticket le pertenezca, o que el usuario sea parte del Staff
            bool esDueño = await _soporteRepo.EsTicketDelUsuarioAsync(idTicket, userId);
            bool esStaff = User.IsInRole("SOP_GESTION_TICKETS") || User.IsInRole("Administrador");

            if (!esDueño && !esStaff)
            {
                return Forbid(); // No está autorizado a ver un ticket que no es suyo
            }

            var detalle = await _soporteRepo.GetTicketDetalleAsync(idTicket);
            if (detalle == null) return NotFound(new { message = "El ticket no existe." });

            return Ok(detalle);
        }

        [Authorize]
        [HttpPost("mensaje")]
        public async Task<IActionResult> AgregarMensaje([FromBody] NuevoMensajeRequestDTO request)
        {
            var userId = ObtenerIdUsuarioActual();

            bool esDueño = await _soporteRepo.EsTicketDelUsuarioAsync(request.IdTicket, userId);
            bool esStaff = User.IsInRole("SOP_GESTION_TICKETS") || User.IsInRole("Administrador");

            if (!esDueño && !esStaff) return Forbid();

            // Si es Staff Y NO es el dueño del ticket, la respuesta cuenta como soporte oficial
            bool esRespuestaSoporte = esStaff && !esDueño;

            var mensaje = await _soporteRepo.AgregarMensajeAsync(request.IdTicket, userId, request.Mensaje, esRespuestaSoporte);
            return Ok(mensaje);
        }

        // =========================================================
        // 3. ENDPOINTS EXCLUSIVOS PARA EL STAFF (Soporte Técnico)
        // =========================================================
        [Authorize(Roles = "SOP_GESTION_TICKETS, Administrador, Secretaria Academica")]
        [HttpGet("admin/todos")]
        public async Task<IActionResult> GetAllTickets([FromQuery] string estado = "Todos", [FromQuery] string? busqueda = null, [FromQuery] DateTime? fechaDesde = null, [FromQuery] DateTime? fechaHasta = null, [FromQuery] int limite = 10)
        {
            var tickets = await _soporteRepo.GetTodosLosTicketsAsync(estado, busqueda, fechaDesde, fechaHasta, limite);
            return Ok(tickets);
        }

        [Authorize(Roles = "SOP_GESTION_TICKETS, Administrador, Secretaria Academica")]
        [HttpPut("admin/{idTicket}/estado")]
        public async Task<IActionResult> CambiarEstadoTicket(int idTicket, [FromBody] string nuevoEstado)
        {
            if (nuevoEstado != "Abierto" && nuevoEstado != "Pendiente" && nuevoEstado != "Cerrado")
                return BadRequest(new { message = "Estado no válido." });

            var exito = await _soporteRepo.CambiarEstadoTicketAsync(idTicket, nuevoEstado);
            if (!exito) return NotFound(new { message = "El ticket no existe." });

            return Ok(new { message = $"El ticket fue marcado como {nuevoEstado}." });
        }

        // --- Helper ---
        private int ObtenerIdUsuarioActual()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(claim, out int userId);
            return userId;
        }
    }
}