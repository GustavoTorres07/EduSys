using EduSys.Api.Data;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
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

        // ========================================================
        // 1. MÉTODOS DE OBTENCIÓN Y LECTURA (USADOS POR LA UI)
        // ========================================================
        public async Task<List<NotificacionDTO>> GetNotificacionesByUsuarioAsync(int idUsuario)
        {
            return await _context.Notificacions
                .AsNoTracking()
                .Where(n => n.IdUsuario == idUsuario)
                .OrderByDescending(n => n.Fecha)
                .Take(50)
                .Select(n => new NotificacionDTO
                {
                    Id = n.Id,
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    // ✅ CORRECCIÓN: Quitamos el '??' porque en tu modelo no son nullable
                    Fecha = n.Fecha,
                    Leida = n.Leida,
                    Tipo = n.Tipo ?? "Sistema"
                })
                .ToListAsync();
        }

        public async Task<bool> MarcarLeidaAsync(int idNotificacion, int idUsuario)
        {
            var noti = await _context.Notificacions.FirstOrDefaultAsync(n => n.Id == idNotificacion && n.IdUsuario == idUsuario);
            if (noti == null) return false;
            noti.Leida = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarcarTodasLeidasAsync(int idUsuario)
        {
            var noLeidas = await _context.Notificacions.Where(n => n.IdUsuario == idUsuario && n.Leida == false).ToListAsync();
            if (!noLeidas.Any()) return true;
            foreach (var n in noLeidas) n.Leida = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ========================================================
        // 2. DISPARADORES INDIVIDUALES Y DE GESTIÓN (TRIGGERS)
        // ========================================================

        public async Task NotificarCierreActaAsync(int idEvaluacion, string nombreExamen)
        {
            try
            {
                var evaluacion = await _context.Evaluacions.AsNoTracking().FirstOrDefaultAsync(e => e.Id == idEvaluacion);
                if (evaluacion == null) return;

                var idsUsuarios = await _context.InscripcionCursada
                    .AsNoTracking()
                    .Where(i => i.IdComision == evaluacion.IdComision && i.Estado != "Baja")
                    .Select(i => i.IdAlumnoNavigation.IdUsuario)
                    .ToListAsync();

                var notificaciones = idsUsuarios.Select(idUsuario => new Notificacion
                {
                    IdUsuario = idUsuario,
                    Titulo = "Acta Cerrada",
                    Mensaje = $"El acta del examen '{nombreExamen}' ha sido cerrada. Ya puedes consultar tu nota oficial.",
                    Tipo = "Examen",
                    Fecha = DateTime.Now,
                    Leida = false
                }).ToList();

                _context.Notificacions.AddRange(notificaciones);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) { _logger.LogError(ex, "Error en NotificarCierreActaAsync"); }
        }

        public async Task<bool> EnviarNotificacionMasivaAsync(NotificacionMasivaDTO request)
        {
            try
            {
                IQueryable<Usuario> query = _context.Usuarios.Include(u => u.IdRols).Where(u => u.Activo == true);

                if (request.Destinatarios == "Alumnos")
                    query = query.Where(u => u.IdRols.Any(r => r.Nombre == "Alumno"));
                else if (request.Destinatarios == "Docentes")
                    query = query.Where(u => u.IdRols.Any(r => r.Nombre == "Docente"));

                var usuariosIds = await query.Select(u => u.Id).ToListAsync();
                if (!usuariosIds.Any()) return false;

                var notificaciones = usuariosIds.Select(id => new Notificacion
                {
                    IdUsuario = id,
                    Titulo = request.Titulo,
                    Mensaje = request.Mensaje,
                    Tipo = request.Tipo,
                    Fecha = DateTime.Now,
                    Leida = false
                }).ToList();

                _context.Notificacions.AddRange(notificaciones);
                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task NotificarRiesgoInasistenciaAsync(int idAlumno, string materia)
        {
            var alumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.Id == idAlumno);
            if (alumno == null) return;

            var noti = new Notificacion
            {
                IdUsuario = alumno.IdUsuario,
                Titulo = "⚠️ Alerta de Asistencia",
                Mensaje = $"Tu porcentaje de asistencia en la materia '{materia}' está por debajo del límite permitido. Por favor regulariza tu situación.",
                Tipo = "Asistencia",
                Fecha = DateTime.Now,
                Leida = false
            };
            _context.Notificacions.Add(noti);
            await _context.SaveChangesAsync();
        }

        public async Task NotificarAsignacionMesaDocenteAsync(int idDocente, string materia, DateTime fechaExamen)
        {
            var docente = await _context.Docentes.FirstOrDefaultAsync(d => d.Id == idDocente);
            if (docente == null) return;

            var noti = new Notificacion
            {
                IdUsuario = docente.IdUsuario,
                Titulo = "Nueva Mesa de Examen Asignada",
                Mensaje = $"Has sido designado para integrar la mesa examinadora de '{materia}' el día {fechaExamen.ToString("dd/MM/yyyy HH:mm")}.",
                Tipo = "Info",
                Fecha = DateTime.Now,
                Leida = false
            };
            _context.Notificacions.Add(noti);
            await _context.SaveChangesAsync();
        }

        // ✅ CORRECCIÓN: Método faltante que exige la interfaz
        public async Task NotificarAperturaInscripcionMateriasAsync(string periodoNombre)
        {
            try
            {
                var alumnosIds = await _context.Usuarios
                    .Where(u => u.Activo == true && u.IdRols.Any(r => r.Nombre == "Alumno"))
                    .Select(u => u.Id)
                    .ToListAsync();

                var notificaciones = alumnosIds.Select(id => new Notificacion
                {
                    IdUsuario = id,
                    Titulo = "¡Inscripciones Abiertas!",
                    Mensaje = $"Ya puedes inscribirte a las materias del período {periodoNombre}. Revisa la oferta académica.",
                    Tipo = "Inscripcion",
                    Fecha = DateTime.Now,
                    Leida = false
                }).ToList();

                _context.Notificacions.AddRange(notificaciones);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Se enviaron {Cant} notificaciones de apertura de inscripción.", notificaciones.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al notificar apertura de inscripciones.");
            }
        }

        // ========================================================
        // 3. MÉTODOS DEL BACKGROUND WORKER (PROACTIVOS)
        // ========================================================
        public async Task GenerarAlertasVencimientoMesasAsync()
        {
            try
            {
                var mananaInicio = DateTime.Today.AddDays(1);
                var mananaFin = mananaInicio.AddDays(1);

                var ventanasCerrando = await _context.VentanaOperativas
                    .AsNoTracking()
                    .Where(v => v.FechaFin >= mananaInicio && v.FechaFin < mananaFin)
                    .ToListAsync();

                if (!ventanasCerrando.Any()) return;

                var idAlumnosActivos = await _context.Usuarios
                    .Where(u => u.Activo == true && u.IdRols.Any(r => r.Nombre == "Alumno"))
                    .Select(u => u.Id)
                    .ToListAsync();

                var nuevasNotificaciones = new List<Notificacion>();

                foreach (var ventana in ventanasCerrando)
                {
                    string accionFormateada = ventana.TipoAccion == "INSCRIPCION_CURSADA" ? "Inscripción a Cursadas" : "Inscripción a Exámenes Finales";

                    foreach (var id in idAlumnosActivos)
                    {
                        nuevasNotificaciones.Add(new Notificacion
                        {
                            IdUsuario = id,
                            Titulo = "¡Último Día de Inscripción!",
                            Mensaje = $"Te recordamos que la ventana operativa para '{accionFormateada}' finaliza mañana. No olvides inscribirte.",
                            Tipo = "Alerta",
                            Fecha = DateTime.Now,
                            Leida = false
                        });
                    }
                }

                if (nuevasNotificaciones.Any())
                {
                    _context.Notificacions.AddRange(nuevasNotificaciones);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ [Worker] Se generaron {Cant} alertas de vencimiento de ventanas.", nuevasNotificaciones.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [Worker] Error al revisar ventanas operativas.");
            }
        }
    }
}