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
            // 1. Traemos los datos con una sola cadena de Includes bien definida
            var horarios = await _context.HorarioComisions
                .Include(h => h.IdAulaNavigation)
                .Include(h => h.IdComisionNavigation)
                    .ThenInclude(c => c.IdSedeNavigation)
                .Include(h => h.IdComisionNavigation)
                    .ThenInclude(c => c.IdPlanMateriaNavigation)
                        .ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(h => h.IdComisionNavigation)
                    .ThenInclude(c => c.IdPlanMateriaNavigation)
                        .ThenInclude(pm => pm.IdPlanNavigation) // Requerido para el filtro de Carrera
                        .ThenInclude(p => p.IdCarreraNavigation) // <--- ¡FALTABA ESTE INCLUDE!

                // 2. Filtros (con validación de nulos preventiva)
                .Where(h => h.IdComisionNavigation != null &&
                            h.IdComisionNavigation.IdPeriodo == idPeriodo &&
                            h.IdComisionNavigation.IdSede == idSede &&
                            h.IdComisionNavigation.Estado == "Abierta" &&
                            h.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarrera == idCarrera)
                .ToListAsync();

            // 3. Proyección segura
            return horarios.Select(h => new HorarioVisualizacionDTO
            {
                Id = h.Id,
                IdComision = h.IdComision,

                // Usamos navegación segura (?.) y operadores null-coalescing (??)
                Materia = h.IdComisionNavigation?.IdPlanMateriaNavigation?.IdMateriaNavigation?.Nombre ?? "Materia sin nombre",
                Curso = h.IdComisionNavigation?.Codigo ?? "S/C",
                ComisionCodigo = h.IdComisionNavigation?.Codigo ?? "S/C",
                AnioCursada = h.IdComisionNavigation?.IdPlanMateriaNavigation?.AnioCursada ?? 0,


                CarreraNombre = h.IdComisionNavigation?.IdPlanMateriaNavigation?.IdPlanNavigation?.IdCarreraNavigation?.Nombre ?? "Carrera Desconocida",
                Dia = h.DiaSemana ?? "Sin Día",
                HoraInicio = h.HoraInicio,
                HoraFin = h.HoraFin,

                Aula = h.IdAulaNavigation?.Nombre ?? "Sin Aula",
                Sede = h.IdComisionNavigation?.IdSedeNavigation?.Nombre ?? "Sin Sede"
            })
            .OrderBy(x => x.AnioCursada)
                .ThenBy(x => x.Curso)
                .ThenBy(x => x.Dia)
                .ThenBy(x => x.HoraInicio)
            .ToList();
        }

        // --- Los otros métodos permanecen igual ---

        public async Task<List<HorarioComision>> GetByComisionAsync(int idComision)
        {
            return await _context.HorarioComisions
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
                .AnyAsync(h => h.IdAula == idAula
                            && h.DiaSemana == dia
                            && inicio < h.HoraFin
                            && fin > h.HoraInicio);
        }

        // EN IHorarioRepository.cs AGREGAR:
        // Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno);

        // EN HorarioRepository.cs IMPLEMENTAR:
        public async Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno)
        {
            // Consultamos los horarios de las comisiones donde el alumno tiene inscripción "Cursando"
            var horarios = await _context.HorarioComisions
                .Include(h => h.IdAulaNavigation)
                .Include(h => h.IdComisionNavigation)
                    .ThenInclude(c => c.IdPlanMateriaNavigation)
                        .ThenInclude(pm => pm.IdMateriaNavigation)
                // FILTRO CLAVE: Solo comisiones donde el alumno está inscripto y activo
                .Where(h => h.IdComisionNavigation.IdPeriodo == idPeriodo
                         && h.IdComisionNavigation.InscripcionCursada
                            .Any(i => i.IdAlumno == idAlumno && i.Estado == "Cursando"))
                .ToListAsync();

            // Mapeo a DTO
            return horarios.Select(h => new HorarioVisualizacionDTO
            {
                Id = h.Id,
                IdComision = h.IdComision,
                Materia = h.IdComisionNavigation?.IdPlanMateriaNavigation?.IdMateriaNavigation?.Nombre ?? "Desconocida",
                Curso = h.IdComisionNavigation?.Codigo ?? "S/C", // Ej: 1° A
                Dia = h.DiaSemana,
                HoraInicio = h.HoraInicio,
                HoraFin = h.HoraFin,
                Aula = h.IdAulaNavigation?.Nombre ?? "Sin Aula Asignada"
            })
            .OrderBy(x => x.HoraInicio)
            .ToList();
        }
    }
}