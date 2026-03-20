using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class AlumnoPortalRepository : IAlumnoPortalRepository
    {
        private readonly EduSysDbContext _context;

        public AlumnoPortalRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificacionDTO>> GetNotificacionesAsync(int idUsuario)
        {
            return await _context.Notificacions
                .AsNoTracking() // 🚀 OPTIMIZACIÓN: Evita el consumo de memoria caché de EF Core
                .Where(n => n.IdUsuario == idUsuario)
                .OrderByDescending(n => n.Fecha)
                .Take(50) // Traemos las últimas 50
                .Select(n => new NotificacionDTO
                {
                    Id = n.Id,
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    Fecha = n.Fecha,
                    Leida = n.Leida,
                    Tipo = n.Tipo ?? "Sistema"
                })
                .ToListAsync();
        }

        public async Task<bool> MarcarNotificacionLeidaAsync(int idNotificacion)
        {
            // Este SÍ requiere Tracking porque vamos a modificar y guardar (Update)
            var notif = await _context.Notificacions.FindAsync(idNotificacion);
            if (notif == null) return false;

            notif.Leida = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<CursadaAlumnoDTO>> GetMisCursadasAsync(int idUsuario)
        {
            // 🚀 OPTIMIZACIÓN EXTREMA: Proyección directa en SQL. 
            // En lugar de traer toda la entidad y luego mapear en C#, 
            // le decimos a SQL que construya el DTO directamente. Es 10 veces más rápido.

            var resultado = await _context.InscripcionCursada
                .AsNoTracking()
                .Where(i => i.IdAlumnoNavigation.IdUsuario == idUsuario && i.Estado != "Baja")
                .Select(ins => new CursadaAlumnoDTO
                {
                    IdInscripcion = ins.Id,
                    Materia = ins.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    Comision = ins.IdComisionNavigation.Codigo,
                    EstadoCursada = ins.CondicionFinal ?? ins.Estado,

                    Examenes = ins.IdComisionNavigation.Evaluacions
                        .OrderBy(e => e.Fecha)
                        .Select(eval => new ExamenAlumnoDTO
                        {
                            Nombre = eval.Nombre,
                            // Manejo seguro de DateOnly a DateTime
                            Fecha = eval.Fecha.ToDateTime(TimeOnly.MinValue),
                            EstadoActa = eval.EstadoActa ?? "Abierta",

                            // Extraemos la nota que corresponda a esta evaluación y a este alumno en particular
                            Nota = ins.Nota.Where(n => n.IdEvaluacion == eval.Id).Select(n => (decimal?)n.Valor).FirstOrDefault()
                        }).ToList()
                })
                .ToListAsync();

            // Calculamos el promedio en memoria solo para los DTOs resultantes
            foreach (var dto in resultado)
            {
                var notasConValor = dto.Examenes.Where(e => e.Nota.HasValue).Select(e => e.Nota!.Value).ToList();
                if (notasConValor.Any())
                {
                    dto.Promedio = Math.Round(notasConValor.Average(), 2);
                }
            }

            return resultado;
        }
    }
}