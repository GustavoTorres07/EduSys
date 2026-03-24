using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class HorarioRepository : IHorarioRepository
    {
        private readonly EduSysDbContext _context;

        public HorarioRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<HorarioVisualizacionDTO>> GetHorariosByCarreraAndPeriodoAsync(int idPeriodo, int idCarrera, int idSede)
        {
            // 🚀 OPTIMIZADO: Proyección directa incluyendo Profesor y Código de Materia
            return await _context.HorarioComisions
                .AsNoTracking()
                .Where(h => h.IdComisionNavigation != null &&
                            h.IdComisionNavigation.IdPeriodo == idPeriodo &&
                            h.IdComisionNavigation.IdSede == idSede &&
                            h.IdComisionNavigation.Estado == "Abierta" &&
                            h.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarrera == idCarrera)
                .Select(h => new HorarioVisualizacionDTO
                {
                    Id = h.Id,
                    IdComision = h.IdComision,
                    Materia = h.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre ?? "Materia sin nombre",
                    // Buscamos el código de la materia
                    Codigo = h.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Codigo ?? "",
                    Curso = h.IdComisionNavigation.Codigo ?? "S/C",
                    ComisionCodigo = h.IdComisionNavigation.Codigo ?? "S/C",
                    AnioCursada = h.IdComisionNavigation.IdPlanMateriaNavigation.AnioCursada,
                    CarreraNombre = h.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarreraNavigation.Nombre ?? "Carrera Desconocida",
                    Dia = h.DiaSemana ?? "Sin Día",
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    Aula = h.IdAulaNavigation != null ? h.IdAulaNavigation.Nombre : "Sin Aula",
                    Sede = h.IdComisionNavigation.IdSedeNavigation != null ? h.IdComisionNavigation.IdSedeNavigation.Nombre : "Sin Sede",
                    // Buscamos el apellido y nombre del primer docente activo asignado a la comisión
                    Profesor = h.IdComisionNavigation.DocenteComisions
                                .Where(dc => dc.Activo)
                                .Select(dc => dc.IdDocenteNavigation.IdUsuarioNavigation.Apellido + ", " + dc.IdDocenteNavigation.IdUsuarioNavigation.Nombre)
                                .FirstOrDefault() ?? ""
                })
                .OrderBy(x => x.AnioCursada)
                    .ThenBy(x => x.Curso)
                    .ThenBy(x => x.Dia)
                    .ThenBy(x => x.HoraInicio)
                .ToListAsync();
        }

        public async Task<List<HorarioComision>> GetByComisionAsync(int idComision)
        {
            return await _context.HorarioComisions
                .AsNoTracking()
                .Include(h => h.IdAulaNavigation)
                    .ThenInclude(a => a.IdSedeNavigation)
                .Where(h => h.IdComision == idComision)
                .OrderBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();
        }

        public async Task<bool> CreateAsync(HorarioComision horario)
        {
            if (horario.IdAula.HasValue && horario.IdAula.Value > 0)
            {
                if (await ValidarSuperposicionAsync(horario.IdAula.Value, horario.DiaSemana, horario.HoraInicio, horario.HoraFin))
                {
                    throw new InvalidOperationException("El aula ya está ocupada en ese horario.");
                }
            }

            _context.HorarioComisions.Add(horario);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.HorarioComisions.FindAsync(id);
            if (item == null) return false;

            _context.HorarioComisions.Remove(item);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ValidarSuperposicionAsync(int idAula, string dia, TimeSpan inicio, TimeSpan fin)
        {
            return await _context.HorarioComisions
                .AsNoTracking()
                .AnyAsync(h => h.IdAula == idAula
                            && h.DiaSemana == dia
                            && inicio < h.HoraFin
                            && fin > h.HoraInicio);
        }

        public async Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno)
        {
            // 🚀 OPTIMIZADO: Proyección directa a SQL incluyendo Profesor y Código
            return await _context.HorarioComisions
                .AsNoTracking()
                .Where(h => h.IdComisionNavigation.IdPeriodo == idPeriodo
                         && h.IdComisionNavigation.InscripcionCursada.Any(i => i.IdAlumno == idAlumno && i.Estado == "Cursando"))
                .Select(h => new HorarioVisualizacionDTO
                {
                    Id = h.Id,
                    IdComision = h.IdComision,
                    Materia = h.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre ?? "Desconocida",
                    Codigo = h.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Codigo ?? "",
                    Curso = h.IdComisionNavigation.Codigo ?? "S/C",
                    Dia = h.DiaSemana,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    Aula = h.IdAulaNavigation != null ? h.IdAulaNavigation.Nombre : "Sin Aula Asignada",
                    Profesor = h.IdComisionNavigation.DocenteComisions
                                .Where(dc => dc.Activo)
                                .Select(dc => dc.IdDocenteNavigation.IdUsuarioNavigation.Apellido + ", " + dc.IdDocenteNavigation.IdUsuarioNavigation.Nombre)
                                .FirstOrDefault() ?? ""
                })
                .OrderBy(x => x.HoraInicio)
                .ToListAsync();
        }
    }
}