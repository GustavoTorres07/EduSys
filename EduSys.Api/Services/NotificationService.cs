using EduSys.Api.Data;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduSys.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly EduSysDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(EduSysDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task NotificarCierreActaAsync(int idEvaluacion, string nombreExamen)
        {
            try
            {
                // 1. Obtener la comisión de la evaluación (Solo lectura rápida)
                var evaluacion = await _context.Evaluacions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == idEvaluacion);

                if (evaluacion == null)
                {
                    _logger.LogWarning("⚠️ No se encontró la evaluación con ID {IdEvaluacion} para emitir notificaciones.", idEvaluacion);
                    return;
                }

                // 2. 🚀 OPTIMIZACIÓN: Obtener SOLO los IDs de usuario (Evita cargar objetos pesados en RAM)
                var idsUsuarios = await _context.InscripcionCursada
                    .AsNoTracking()
                    .Where(i => i.IdComision == evaluacion.IdComision && i.Estado != "Baja")
                    .Select(i => i.IdAlumnoNavigation.IdUsuario)
                    .ToListAsync();

                if (!idsUsuarios.Any())
                {
                    _logger.LogInformation("ℹ️ No hay alumnos activos en la comisión {IdComision} para notificar.", evaluacion.IdComision);
                    return;
                }

                // 3. Crear notificaciones masivas usando LINQ
                var notificaciones = idsUsuarios.Select(idUsuario => new Notificacion
                {
                    IdUsuario = idUsuario,
                    Titulo = "Acta Cerrada",
                    Mensaje = $"El acta del examen '{nombreExamen}' ha sido cerrada. Ya puedes consultar tu nota oficial.",
                    Tipo = "Examen",
                    Fecha = DateTime.Now,
                    Leida = false
                }).ToList();

                // 4. Guardar en bloque
                _context.Notificacions.AddRange(notificaciones);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Se enviaron {Cantidad} notificaciones por el cierre del acta '{NombreExamen}'.", notificaciones.Count, nombreExamen);
            }
            catch (Exception ex)
            {
                // Registramos el error pero no lo lanzamos (throw), 
                // para evitar que un fallo en las notificaciones rompa el cierre del acta principal.
                _logger.LogError(ex, "❌ Error al generar notificaciones para el cierre del acta de la evaluación {IdEvaluacion}.", idEvaluacion);
            }
        }
    }
}