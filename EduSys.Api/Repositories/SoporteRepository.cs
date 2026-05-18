using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class SoporteRepository : ISoporteRepository
    {
        private readonly EduSysDbContext _context;
        private readonly IEmailService _emailService;

        // ✅ Inyectamos IEmailService en el constructor
        public SoporteRepository(EduSysDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<int?> ObtenerIdUsuarioPorIdentificacionAsync(string identificacion)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => (u.Email == identificacion || u.Dni == identificacion) && u.Activo == true);

            return usuario?.Id;
        }

        public async Task<SoporteTicketDTO> CrearTicketAsync(int idUsuario, string categoria, string asunto, string mensaje)
        {
            var year = DateTime.Now.Year;
            var conteoAnual = await _context.SoporteTickets.CountAsync(t => t.FechaCreacion.Year == year);
            var numeroTicket = $"TK-{year}-{(conteoAnual + 1):D4}";

            var ticket = new SoporteTicket
            {
                NumeroTicket = numeroTicket,
                IdUsuario = idUsuario,
                Categoria = categoria,
                Asunto = asunto,
                Estado = "Abierto",
                FechaCreacion = DateTime.Now
            };

            _context.SoporteTickets.Add(ticket);
            await _context.SaveChangesAsync();

            var primerMensaje = new SoporteMensaje
            {
                IdTicket = ticket.Id,
                IdUsuario = idUsuario,
                Mensaje = mensaje,
                Fecha = DateTime.Now,
                EsRespuestaSoporte = false
            };

            _context.SoporteMensajes.Add(primerMensaje);
            await _context.SaveChangesAsync();

            // 🚀 ENVÍO DE EMAIL: TICKET CREADO
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario != null && !string.IsNullOrWhiteSpace(usuario.Email))
            {
                string body = GenerarPlantillaEmail(
                    "¡Ticket de Soporte Recibido!",
                    usuario.Nombre,
                    $"Hemos recibido tu solicitud bajo el número <strong>{numeroTicket}</strong> (Categoría: {categoria}).",
                    "Nuestro equipo de soporte técnico revisará tu consulta a la brevedad y te notificaremos cuando haya una respuesta.");

                // Lo envolvemos en un try-catch para que si falla el email, no se cancele la creación del ticket
                try { await _emailService.SendEmailAsync(usuario.Email, $"EduSys Soporte - Ticket {numeroTicket}", body); } catch { }
            }

            return await ObtenerTicketBasicoAsync(ticket.Id);
        }

        public async Task<SoporteMensajeDTO> AgregarMensajeAsync(int idTicket, int idUsuario, string mensaje, bool esRespuestaSoporte)
        {
            var nuevoMensaje = new SoporteMensaje
            {
                IdTicket = idTicket,
                IdUsuario = idUsuario,
                Mensaje = mensaje,
                Fecha = DateTime.Now,
                EsRespuestaSoporte = esRespuestaSoporte
            };

            _context.SoporteMensajes.Add(nuevoMensaje);

            // Dentro de AgregarMensajeAsync:
            var ticket = await _context.SoporteTickets.Include(t => t.UsuarioNavigation).FirstOrDefaultAsync(t => t.Id == idTicket);

            if (ticket != null)
            {
                // Al agregar un mensaje, actualizamos el estado. Si estaba cerrado, se reabre automáticamente.
                ticket.Estado = esRespuestaSoporte ? "Pendiente" : "Abierto";
                ticket.FechaCierre = null; // Limpiamos la fecha de cierre por si estaba cerrado
            }

            await _context.SaveChangesAsync();

            var autor = await _context.Usuarios.FindAsync(idUsuario);

            // 🚀 ENVÍO DE EMAIL: SOPORTE RESPONDIÓ
            if (esRespuestaSoporte && ticket?.UsuarioNavigation != null && !string.IsNullOrWhiteSpace(ticket.UsuarioNavigation.Email))
            {
                string body = GenerarPlantillaEmail(
                    "Nueva respuesta en tu ticket",
                    ticket.UsuarioNavigation.Nombre,
                    $"El equipo de soporte ha respondido a tu ticket <strong>{ticket.NumeroTicket}</strong> ({ticket.Asunto}).",
                    $"<strong>Respuesta de soporte:</strong><br/><br/><em>\"{mensaje}\"</em><br/><br/>Por favor, ingresa al portal de EduSys para continuar la conversación o dar por solucionado el problema.");

                try { await _emailService.SendEmailAsync(ticket.UsuarioNavigation.Email, $"EduSys Soporte - Respuesta al Ticket {ticket.NumeroTicket}", body); } catch { }
            }

            return new SoporteMensajeDTO
            {
                Id = nuevoMensaje.Id,
                IdTicket = nuevoMensaje.IdTicket,
                NombreAutor = autor != null ? $"{autor.Nombre} {autor.Apellido}" : "Sistema",
                Mensaje = nuevoMensaje.Mensaje,
                Fecha = nuevoMensaje.Fecha,
                EsRespuestaSoporte = nuevoMensaje.EsRespuestaSoporte
            };
        }

        public async Task<bool> CambiarEstadoTicketAsync(int idTicket, string nuevoEstado)
        {
            var ticket = await _context.SoporteTickets.Include(t => t.UsuarioNavigation).FirstOrDefaultAsync(t => t.Id == idTicket);
            if (ticket == null) return false;

            ticket.Estado = nuevoEstado;
            ticket.FechaCierre = (nuevoEstado == "Cerrado") ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            // 🚀 ENVÍO DE EMAIL: CAMBIO DE ESTADO (CERRADO O REABIERTO)
            if (ticket.UsuarioNavigation != null && !string.IsNullOrWhiteSpace(ticket.UsuarioNavigation.Email))
            {
                string tituloEmail = nuevoEstado == "Cerrado" ? "Tu ticket ha sido Resuelto" : "Tu ticket ha sido Reabierto";
                string textoPrincipal = nuevoEstado == "Cerrado"
                    ? $"Te informamos que tu ticket <strong>{ticket.NumeroTicket}</strong> ha sido marcado como Resuelto y Cerrado por nuestro equipo."
                    : $"Te informamos que tu ticket <strong>{ticket.NumeroTicket}</strong> ha sido reabierto por el área de soporte para continuar con la atención.";

                string textoSecundario = nuevoEstado == "Cerrado"
                    ? "Si continúas teniendo problemas, puedes iniciar sesión en la plataforma y reabrir este ticket en un periodo de 72hs."
                    : "Por favor, mantente atento a tu bandeja de tickets en EduSys.";

                string body = GenerarPlantillaEmail(tituloEmail, ticket.UsuarioNavigation.Nombre, textoPrincipal, textoSecundario);

                try { await _emailService.SendEmailAsync(ticket.UsuarioNavigation.Email, $"EduSys Soporte - Ticket {ticket.NumeroTicket} {nuevoEstado}", body); } catch { }
            }

            return true;
        }

        public async Task<List<SoporteTicketDTO>> GetTicketsPorUsuarioAsync(int idUsuario)
        {
            return await MapearConsultaTicket(_context.SoporteTickets.Where(t => t.IdUsuario == idUsuario)).ToListAsync();
        }

        public async Task<List<SoporteTicketDTO>> GetTodosLosTicketsAsync(string? estado = null, string? busqueda = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int limite = 10)
        {
            var query = _context.SoporteTickets.AsQueryable();

            if (!string.IsNullOrEmpty(estado) && estado != "Todos") query = query.Where(t => t.Estado == estado);

            if (!string.IsNullOrEmpty(busqueda))
            {
                var b = busqueda.ToLower();
                query = query.Where(t => t.NumeroTicket.ToLower().Contains(b) || t.Asunto.ToLower().Contains(b) || t.UsuarioNavigation.Nombre.ToLower().Contains(b) || t.UsuarioNavigation.Apellido.ToLower().Contains(b) || t.UsuarioNavigation.Email.ToLower().Contains(b));
            }

            if (fechaDesde.HasValue) query = query.Where(t => t.FechaCreacion.Date >= fechaDesde.Value.Date);
            if (fechaHasta.HasValue) query = query.Where(t => t.FechaCreacion.Date <= fechaHasta.Value.Date);

            return await MapearConsultaTicket(query).Take(limite).ToListAsync();
        }

        public async Task<SoporteTicketDetalleDTO?> GetTicketDetalleAsync(int idTicket)
        {
            var ticketDto = await ObtenerTicketBasicoAsync(idTicket);
            if (ticketDto == null) return null;

            var mensajes = await _context.SoporteMensajes
                .Include(m => m.UsuarioNavigation)
                .Where(m => m.IdTicket == idTicket)
                .OrderBy(m => m.Fecha)
                .Select(m => new SoporteMensajeDTO
                {
                    Id = m.Id,
                    IdTicket = m.IdTicket,
                    NombreAutor = m.UsuarioNavigation.Nombre + " " + m.UsuarioNavigation.Apellido,
                    Mensaje = m.Mensaje,
                    Fecha = m.Fecha,
                    EsRespuestaSoporte = m.EsRespuestaSoporte
                }).ToListAsync();

            return new SoporteTicketDetalleDTO { Ticket = ticketDto, HistorialMensajes = mensajes };
        }

        public async Task<bool> EsTicketDelUsuarioAsync(int idTicket, int idUsuario)
        {
            return await _context.SoporteTickets.AnyAsync(t => t.Id == idTicket && t.IdUsuario == idUsuario);
        }

        private async Task<SoporteTicketDTO> ObtenerTicketBasicoAsync(int idTicket)
        {
            return await MapearConsultaTicket(_context.SoporteTickets.Where(t => t.Id == idTicket)).FirstOrDefaultAsync();
        }

        private IQueryable<SoporteTicketDTO> MapearConsultaTicket(IQueryable<SoporteTicket> query)
        {
            return query
                .Include(t => t.UsuarioNavigation)
                .OrderByDescending(t => t.FechaCreacion)
                .Select(t => new SoporteTicketDTO
                {
                    Id = t.Id,
                    NumeroTicket = t.NumeroTicket,
                    IdUsuario = t.IdUsuario,
                    NombreSolicitante = t.UsuarioNavigation.Nombre + " " + t.UsuarioNavigation.Apellido,
                    EmailSolicitante = t.UsuarioNavigation.Email,
                    Categoria = t.Categoria,
                    Asunto = t.Asunto,
                    Estado = t.Estado,
                    FechaCreacion = t.FechaCreacion,
                    FechaCierre = t.FechaCierre
                });
        }

        // --- HELPER PARA GENERAR EMAILS BONITOS EN HTML ---
        private string GenerarPlantillaEmail(string titulo, string nombreUsuario, string textoPrincipal, string textoSecundario)
        {
            return $@"
            <div style='font-family: ""Segoe UI"", Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #ffffff; border: 1px solid #dde3ea; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05);'>
                <div style='background-color: #456990; padding: 25px 20px; text-align: center;'>
                    <h2 style='color: #ffffff; margin: 0; font-size: 24px; font-weight: 700; letter-spacing: 0.5px;'>EduSys Soporte</h2>
                </div>
                
                <div style='padding: 30px 25px; color: #1a2d45;'>
                    <h3 style='margin-top: 0; color: #2b4162; font-size: 20px;'>{titulo}</h3>
                    <p style='font-size: 16px; margin-bottom: 20px;'>Hola <strong>{nombreUsuario}</strong>,</p>
                    
                    <div style='background-color: #f8fafc; padding: 15px; border-left: 4px solid #49BEAA; border-radius: 4px; margin-bottom: 20px;'>
                        <p style='margin: 0; font-size: 15px; line-height: 1.5;'>{textoPrincipal}</p>
                    </div>

                    <p style='font-size: 15px; color: #4a5568; line-height: 1.6;'>{textoSecundario}</p>
                    
                    <div style='margin-top: 30px; text-align: center;'>
                        <a href='https://tudominio.com/login' style='background-color: #EF767A; color: #ffffff; padding: 12px 25px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Ir al Portal de EduSys</a>
                    </div>
                </div>
                
                <div style='background-color: #f1f5f9; padding: 15px 20px; text-align: center; border-top: 1px solid #dde3ea;'>
                    <p style='margin: 0; font-size: 12px; color: #64748b;'>Este es un mensaje automático generado por EduSys. Por favor, no respondas directamente a este correo.</p>
                </div>
            </div>";
        }
    }
}