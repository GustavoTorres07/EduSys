using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class ComisionRepository : IComisionRepository
    {
        private readonly EduSysDbContext _context;

        public ComisionRepository(EduSysDbContext context)
        {
            _context = context;
        }

        // ------------------ CRUD ------------------

        public async Task<List<Comision>> GetAllAsync()
        {
            return await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation)
                    .ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(c => c.IdPeriodoNavigation)
                .Include(c => c.IdSedeNavigation)
                .Include(c => c.HorarioComisions)
                .Include(c => c.DocenteComisions)
                    .ThenInclude(dc => dc.IdDocenteNavigation)
                        .ThenInclude(d => d.IdUsuarioNavigation)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Comision>> GetByPeriodoAsync(int idPeriodo)
        {
            return await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation)
                    .ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(c => c.IdPeriodoNavigation)
                .Include(c => c.IdSedeNavigation)
                .Where(c => c.IdPeriodo == idPeriodo)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Comision?> GetByIdAsync(int id)
        {
            return await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation)
                    .ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(c => c.IdPeriodoNavigation)
                .Include(c => c.IdSedeNavigation)
                .Include(c => c.HorarioComisions)
                .Include(c => c.DocenteComisions)
                    .ThenInclude(dc => dc.IdDocenteNavigation)
                        .ThenInclude(d => d.IdUsuarioNavigation)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> CreateAsync(Comision comision)
        {
            _context.Comisions.Add(comision);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Comision comision)
        {
            var existing = await _context.Comisions.FirstOrDefaultAsync(c => c.Id == comision.Id);
            if (existing == null) return false;

            existing.Codigo = comision.Codigo;
            existing.IdPlanMateria = comision.IdPlanMateria;
            existing.IdPeriodo = comision.IdPeriodo;
            existing.IdSede = comision.IdSede;
            existing.CupoMaximo = comision.CupoMaximo;
            existing.Turno = comision.Turno;
            existing.Estado = comision.Estado;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // ✅ AHORA ES UNA ELIMINACIÓN FÍSICA
            var existing = await _context.Comisions
                .Include(c => c.HorarioComisions)
                .Include(c => c.DocenteComisions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null) return false;

            // Borramos primero los horarios y asignaciones docentes para que SQL no tire error de Foreign Key
            if (existing.HorarioComisions != null)
                _context.HorarioComisions.RemoveRange(existing.HorarioComisions);

            if (existing.DocenteComisions != null)
                _context.DocenteComisions.RemoveRange(existing.DocenteComisions);

            // Borramos la comisión
            _context.Comisions.Remove(existing);

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                // Si entra aquí, es porque tiene Alumnos inscriptos o Notas. El sistema la protege.
                return false;
            }
        }

        // ------------------ CONSULTA BASE (para Controller) ------------------

        public async Task<List<Comision>> GetByPeriodoAndCarreraAsync(int idPeriodo, int idCarrera)
        {
            return await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation)
                    .ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(c => c.IdPlanMateriaNavigation.IdPlanNavigation)
                .Include(c => c.IdPeriodoNavigation)
                .Include(c => c.IdSedeNavigation)
                .Include(c => c.HorarioComisions)
                .Include(c => c.DocenteComisions)
                    .ThenInclude(dc => dc.IdDocenteNavigation)
                        .ThenInclude(d => d.IdUsuarioNavigation)
                .Where(c =>
                    c.IdPeriodo == idPeriodo &&
                    c.IdPlanMateriaNavigation.IdPlanNavigation.IdCarrera == idCarrera) // ✅ Se quitó c.Estado == "Abierta"
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }

        // ------------------ DOCENTES ------------------

        public async Task<List<DocenteComisionListadoDTO>> GetDocentesPorComisionAsync(int idComision)
        {
            return await _context.DocenteComisions
                .AsNoTracking()
                .Where(dc => dc.IdComision == idComision && dc.Activo == true)
                .Select(dc => new DocenteComisionListadoDTO
                {
                    Id = dc.Id,
                    IdDocente = dc.IdDocente,
                    Legajo = dc.IdDocenteNavigation.Legajo,
                    NombreDocente = $"{dc.IdDocenteNavigation.IdUsuarioNavigation.Apellido}, {dc.IdDocenteNavigation.IdUsuarioNavigation.Nombre}",
                    Rol = dc.RolDocente
                })
                .ToListAsync();
        }

        public async Task<bool> AsignarDocenteAsync(DocenteComisionRequestDTO dto)
        {
            var existe = await _context.DocenteComisions
                .AsNoTracking()
                .AnyAsync(dc =>
                    dc.IdComision == dto.IdComision &&
                    dc.IdDocente == dto.IdDocente &&
                    dc.Activo);

            if (existe)
                throw new Exception("El docente ya está asignado a esta comisión.");

            var mensajeConflicto = await ObtenerConflictoHorarioDocenteAsync(dto.IdDocente, dto.IdComision);

            if (mensajeConflicto != null)
                throw new Exception(mensajeConflicto);

            var relacion = new DocenteComision
            {
                IdComision = dto.IdComision,
                IdDocente = dto.IdDocente,
                RolDocente = ObtenerNombreRol(dto.IdRolDocente),
                Activo = true
            };

            _context.DocenteComisions.Add(relacion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DesasignarDocenteAsync(int idDocenteComision)
        {
            var relacion = await _context.DocenteComisions.FindAsync(idDocenteComision);
            if (relacion == null) return false;

            _context.DocenteComisions.Remove(relacion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<ComisionDTO>> GetDTOByPeriodoAndCarreraAsync(int idPeriodo, int idCarrera)
        {
            return await _context.Comisions
                .AsNoTracking()
                .Where(c =>
                    c.IdPeriodo == idPeriodo &&
                    c.IdPlanMateriaNavigation.IdPlanNavigation.IdCarrera == idCarrera) // ✅ Se quitó c.Estado == "Abierta"
                .Select(c => new ComisionDTO
                {
                    Id = c.Id,
                    Codigo = c.Codigo,
                    IdPlanMateria = c.IdPlanMateria,
                    MateriaNombre = c.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    AnioCursada = c.IdPlanMateriaNavigation.AnioCursada,
                    IdPeriodo = c.IdPeriodo,
                    PeriodoNombre = c.IdPeriodoNavigation.Nombre,
                    IdSede = c.IdSede,
                    SedeNombre = c.IdSedeNavigation.Nombre,
                    CupoMaximo = c.CupoMaximo,
                    Turno = c.Turno,
                    Estado = c.Estado,
                    Profesor = c.DocenteComisions
                        .Where(d => d.RolDocente == "Titular" && d.Activo)
                        .Select(d =>
                            d.IdDocenteNavigation.IdUsuarioNavigation.Apellido + ", " +
                            d.IdDocenteNavigation.IdUsuarioNavigation.Nombre)
                        .FirstOrDefault() ?? "Profesor aún no asignado"
                })
                .OrderBy(c => c.AnioCursada)
                .ThenBy(c => c.MateriaNombre)
                .ToListAsync();
        }

        public async Task<List<ComisionDTO>> GetPorSedeAsync(int idSede)
        {
            // ✅ Se quitó el Where estricto de Abierta, para que la UI decida qué mostrar.
            var query = _context.Comisions.AsNoTracking();

            if (idSede > 0)
            {
                query = query.Where(c => c.IdSede == idSede);
            }

            var resultado = await query.Select(c => new ComisionDTO
            {
                Id = c.Id,
                Codigo = c.Codigo ?? "S/C",
                IdPlanMateria = c.IdPlanMateria,
                IdSede = c.IdSede,
                CupoMaximo = c.CupoMaximo,
                Turno = c.Turno ?? "N/A",
                Estado = c.Estado,

                Materia = c.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre ?? "Materia Desconocida",

                Horario = string.Join(", ", c.HorarioComisions
                    .Select(h => h.DiaSemana + " " + h.HoraInicio.ToString(@"hh\:mm") + "-" + h.HoraFin.ToString(@"hh\:mm"))),

                CupoActual = c.InscripcionCursada.Count(i => i.Estado != "Baja")
            })
            .OrderBy(r => r.Materia)
            .ToListAsync();

            foreach (var item in resultado)
            {
                if (string.IsNullOrEmpty(item.Horario)) item.Horario = "Sin horario asignado";
            }

            return resultado;
        }

        private async Task<string?> ObtenerConflictoHorarioDocenteAsync(int idDocente, int idComisionNueva)
        {
            var horariosNuevaComision = await _context.HorarioComisions
                .AsNoTracking()
                .Where(h => h.IdComision == idComisionNueva)
                .ToListAsync();

            if (!horariosNuevaComision.Any()) return null;

            var agendaDocente = await _context.DocenteComisions
                .AsNoTracking()
                .Where(dc => dc.IdDocente == idDocente && dc.Activo && dc.IdComision != idComisionNueva)
                .SelectMany(dc => dc.IdComisionNavigation.HorarioComisions.Select(h => new
                {
                    Materia = dc.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    Comision = dc.IdComisionNavigation.Codigo,
                    Dia = h.DiaSemana,
                    Inicio = h.HoraInicio,
                    Fin = h.HoraFin
                }))
                .ToListAsync();

            foreach (var ocupado in agendaDocente)
            {
                foreach (var nuevo in horariosNuevaComision)
                {
                    if (ocupado.Dia == nuevo.DiaSemana)
                    {
                        if (nuevo.HoraInicio < ocupado.Fin && ocupado.Inicio < nuevo.HoraFin)
                        {
                            return $"Conflicto horario: el docente ya dicta {ocupado.Materia} ({ocupado.Comision}) los {ocupado.Dia} de {ocupado.Inicio:hh\\:mm} a {ocupado.Fin:hh\\:mm}.";
                        }
                    }
                }
            }

            return null;
        }

        private string ObtenerNombreRol(int idRol)
        {
            return idRol switch
            {
                1 => "Titular",
                2 => "Adjunto",
                3 => "Jefe de Trabajos Prácticos",
                4 => "Ayudante",
                _ => "Docente"
            };
        }
    }
}