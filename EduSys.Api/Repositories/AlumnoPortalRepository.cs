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
            var notif = await _context.Notificacions.FindAsync(idNotificacion);
            if (notif == null) return false;

            notif.Leida = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<CursadaAlumnoDTO>> GetMisCursadasAsync(int idUsuario)
        {
            // 1. Buscamos al alumno por su ID de Usuario
            var alumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);
            if (alumno == null) return new List<CursadaAlumnoDTO>();

            // 2. Traemos inscripciones activas (no dadas de baja)
            var inscripciones = await _context.InscripcionCursada
                .Include(i => i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation)
                .Include(i => i.IdComisionNavigation.Evaluacions)
                .Include(i => i.Nota)
                .Where(i => i.IdAlumno == alumno.Id && i.Estado != "Baja")
                .ToListAsync();

            var resultado = new List<CursadaAlumnoDTO>();

            foreach (var ins in inscripciones)
            {
                var dto = new CursadaAlumnoDTO
                {
                    IdInscripcion = ins.Id,
                    Materia = ins.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    Comision = ins.IdComisionNavigation.Codigo,
                    EstadoCursada = ins.CondicionFinal ?? ins.Estado,
                    Examenes = new List<ExamenAlumnoDTO>()
                };

                // Mapeamos los exámenes y buscamos la nota del alumno
                foreach (var eval in ins.IdComisionNavigation.Evaluacions.OrderBy(e => e.Fecha))
                {
                    var nota = ins.Nota.FirstOrDefault(n => n.IdEvaluacion == eval.Id)?.Valor;

                    dto.Examenes.Add(new ExamenAlumnoDTO
                    {
                        Nombre = eval.Nombre,
                        Fecha = eval.Fecha.ToDateTime(TimeOnly.MinValue),
                        EstadoActa = eval.EstadoActa ?? "Abierta",
                        Nota = nota // Aquí podrías poner lógica: si EstadoActa == "Abierta" -> nota = null (si quisieras ocultarlas)
                    });
                }

                // Calcular promedio simple visual
                var notasConValor = dto.Examenes.Where(e => e.Nota.HasValue).Select(e => e.Nota!.Value).ToList();
                if (notasConValor.Any())
                    dto.Promedio = Math.Round(notasConValor.Average(), 2);

                resultado.Add(dto);
            }

            return resultado;
        }
    }
}