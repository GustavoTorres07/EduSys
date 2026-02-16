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
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> CreateAsync(Comision comision)
        {
            _context.Comisions.Add(comision);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Comision comision)
        {
            var existing = await _context.Comisions.FindAsync(comision.Id);
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
            var existing = await _context.Comisions.FindAsync(id);
            if (existing == null) return false;

            existing.Estado = "Cancelada";
            return await _context.SaveChangesAsync() > 0;
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
                    c.IdPlanMateriaNavigation.IdPlanNavigation.IdCarrera == idCarrera &&
                    c.Estado == "Abierta")
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }

        // ------------------ DOCENTES ------------------

        public async Task<List<DocenteComisionListadoDTO>> GetDocentesPorComisionAsync(int idComision)
        {
            var lista = await _context.DocenteComisions
                .Include(dc => dc.IdDocenteNavigation)
                    .ThenInclude(d => d.IdUsuarioNavigation)
                .Where(dc => dc.IdComision == idComision && dc.Activo == true)
                .ToListAsync();

            return lista.Select(dc => new DocenteComisionListadoDTO
            {
                Id = dc.Id,
                IdDocente = dc.IdDocente,
                Legajo = dc.IdDocenteNavigation.Legajo,
                NombreDocente = $"{dc.IdDocenteNavigation.IdUsuarioNavigation.Apellido}, {dc.IdDocenteNavigation.IdUsuarioNavigation.Nombre}",
                Rol = dc.RolDocente
            }).ToList();
        }

        public async Task<bool> AsignarDocenteAsync(DocenteComisionRequestDTO dto)
        {
            var existe = await _context.DocenteComisions
                .AnyAsync(dc =>
                    dc.IdComision == dto.IdComision &&
                    dc.IdDocente == dto.IdDocente &&
                    dc.Activo);

            if (existe)
                throw new Exception("El docente ya está asignado a esta comisión.");

            var mensajeConflicto = await ObtenerConflictoHorarioDocenteAsync(
                dto.IdDocente,
                dto.IdComision);

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

        public async Task<List<ComisionDTO>> GetDTOByPeriodoAndCarreraAsync(
    int idPeriodo,
    int idCarrera)
        {
            return await _context.Comisions
                .Where(c =>
                    c.IdPeriodo == idPeriodo &&
                    c.IdPlanMateriaNavigation.IdPlanNavigation.IdCarrera == idCarrera &&
                    c.Estado == "Abierta")
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
                .AsNoTracking()
                .ToListAsync();
        }

        private async Task<bool> DocenteTieneSolapamientoAsync(
    int idDocente,
    int idComisionNueva)
        {
            // Horarios de la comisión a asignar
            var horariosNuevaComision = await _context.HorarioComisions
                .Where(h => h.IdComision == idComisionNueva)
                .ToListAsync();

            if (!horariosNuevaComision.Any())
                return false; // sin horarios → no hay conflicto

            // Todas las asignaciones activas del docente
            return await _context.DocenteComisions
                .Where(dc =>
                    dc.IdDocente == idDocente &&
                    dc.Activo &&
                    dc.IdComision != idComisionNueva)
                .AnyAsync(dc =>
                    dc.IdComisionNavigation.HorarioComisions.Any(hExistente =>
                        horariosNuevaComision.Any(hNueva =>
                            hExistente.DiaSemana == hNueva.DiaSemana &&
                            hNueva.HoraInicio < hExistente.HoraFin &&
                            hExistente.HoraInicio < hNueva.HoraFin
                        )
                    )
                );
        }


        public async Task<List<ComisionDTO>> GetPorSedeAsync(int idSede)
        {
            // Traemos las comisiones de la sede, incluyendo relaciones necesarias
            var query = await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation)
                    .ThenInclude(pm => pm.IdMateriaNavigation) // Para sacar el nombre de la materia
                .Include(c => c.HorarioComisions) // Para armar el string de horario
                .Include(c => c.IdPeriodoNavigation)
                .Where(c => c.IdSede == idSede && c.Estado == "Abierta" && c.IdPeriodoNavigation.Estado == "Abierto")
                .ToListAsync();

            // Mapeamos a DTO
            var resultado = new List<ComisionDTO>();

            foreach (var c in query)
            {
                // Calculamos inscriptos actuales (Cursando)
                int inscriptos = await _context.InscripcionCursada
                    .CountAsync(i => i.IdComision == c.Id && i.Estado != "Baja");

                // Formateamos horario (Ej: "Lun 18:00-22:00")
                var horariosStr = string.Join(", ", c.HorarioComisions
                    .Select(h => $"{h.DiaSemana} {h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm}"));

                resultado.Add(new ComisionDTO
                {
                    Id = c.Id,
                    Codigo = c.Codigo,
                    IdPlanMateria = c.IdPlanMateria,
                    IdSede = c.IdSede,
                    CupoMaximo = c.CupoMaximo,
                    Turno = c.Turno,
                    Estado = c.Estado,

                    // PROPIEDADES EXTRA PARA EL MODAL ADMIN
                    Materia = c.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    Horario = horariosStr,
                    CupoActual = inscriptos
                });
            }

            return resultado;
        }
        private async Task<string?> ObtenerConflictoHorarioDocenteAsync(int idDocente, int idComisionNueva)
        {
            // A. Obtenemos los horarios de la NUEVA comisión (Ya lo tienes en memoria o lo traemos)
            var horariosNuevaComision = await _context.HorarioComisions
                .Where(h => h.IdComision == idComisionNueva)
                .ToListAsync();

            if (!horariosNuevaComision.Any()) return null;

            // B. TRAEMOS TODOS LOS HORARIOS DEL DOCENTE A MEMORIA (Aquí evitamos el error de traducción LINQ)
            // Seleccionamos solo los datos necesarios para comparar y mostrar el mensaje.
            var agendaDocente = await _context.DocenteComisions
                .Where(dc => dc.IdDocente == idDocente && dc.Activo && dc.IdComision != idComisionNueva)
                .SelectMany(dc => dc.IdComisionNavigation.HorarioComisions.Select(h => new
                {
                    Materia = dc.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    Comision = dc.IdComisionNavigation.Codigo,
                    Dia = h.DiaSemana,
                    Inicio = h.HoraInicio,
                    Fin = h.HoraFin
                }))
                .ToListAsync(); // <--- ¡ESTO ES LO IMPORTANTE! Ejecuta la query aquí.

            // C. COMPARACIÓN EN MEMORIA (C# puro)
            foreach (var ocupado in agendaDocente)
            {
                foreach (var nuevo in horariosNuevaComision)
                {
                    // 1. Mismo día
                    if (ocupado.Dia == nuevo.DiaSemana)
                    {
                        // 2. Superposición de horas: (InicioA < FinB) Y (InicioB < FinA)
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
