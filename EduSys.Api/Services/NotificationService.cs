using EduSys.Api.Data;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly EduSysDbContext _context;

        public NotificationService(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task NotificarCierreActaAsync(int idEvaluacion, string nombreExamen)
        {
            // 1. Obtener la evaluación para saber la comisión
            var evaluacion = await _context.Evaluacions.FindAsync(idEvaluacion);
            if (evaluacion == null) return;

            // 2. Obtener alumnos activos de esa comisión
            var inscripciones = await _context.InscripcionCursada
                .Include(i => i.IdAlumnoNavigation)
                .Where(i => i.IdComision == evaluacion.IdComision && i.Estado != "Baja")
                .ToListAsync();

            // 3. Crear notificaciones masivas
            var notificaciones = new List<Notificacion>();
            foreach (var inscripcion in inscripciones)
            {
                notificaciones.Add(new Notificacion
                {
                    IdUsuario = inscripcion.IdAlumnoNavigation.IdUsuario,
                    Titulo = "Acta Cerrada",
                    Mensaje = $"El acta del examen '{nombreExamen}' ha sido cerrada. Ya puedes consultar tu nota oficial.",
                    Tipo = "Examen",
                    Fecha = DateTime.Now,
                    Leida = false
                });
            }

            if (notificaciones.Any())
            {
                _context.Notificacions.AddRange(notificaciones);
                await _context.SaveChangesAsync();
            }
        }
    }
}