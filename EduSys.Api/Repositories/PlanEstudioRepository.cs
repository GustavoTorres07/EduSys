using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class PlanEstudioRepository : IPlanEstudioRepository
    {
        private readonly EduSysDbContext _context;

        public PlanEstudioRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlanEstudioDTO>> GetAllAsync()
        {
            // 🚀 OPTIMIZADO: Proyección directa en SQL para evitar carga de objetos pesados
            return await _context.PlanEstudios
                .AsNoTracking()
                .OrderByDescending(p => p.AnioInicio)
                .Select(p => new PlanEstudioDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    IdCarrera = p.IdCarrera,
                    NombreCarrera = p.IdCarreraNavigation != null ? p.IdCarreraNavigation.Nombre : "Sin Carrera",
                    AnioInicio = p.AnioInicio,
                    EsVigente = p.EsVigente ?? false,
                    ResolucionMinisterial = p.ResolucionMinisterial,
                    CantidadMaterias = p.PlanMateria.Count
                })
                .ToListAsync();
        }

        public async Task<PlanEstudioDTO?> GetByIdAsync(int id)
        {
            return await _context.PlanEstudios
                .AsNoTracking()
                .Where(plan => plan.Id == id)
                .Select(p => new PlanEstudioDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    IdCarrera = p.IdCarrera,
                    NombreCarrera = p.IdCarreraNavigation != null ? p.IdCarreraNavigation.Nombre : "Sin Carrera",
                    AnioInicio = p.AnioInicio,
                    EsVigente = p.EsVigente ?? false,
                    ResolucionMinisterial = p.ResolucionMinisterial,
                    CantidadMaterias = p.PlanMateria.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(PlanEstudioDTO dto)
        {
            var plan = new PlanEstudio
            {
                Nombre = dto.Nombre,
                IdCarrera = dto.IdCarrera,
                AnioInicio = dto.AnioInicio,
                EsVigente = dto.EsVigente,
                ResolucionMinisterial = dto.ResolucionMinisterial
            };

            _context.PlanEstudios.Add(plan);
            await _context.SaveChangesAsync();
            return plan.Id;
        }

        public async Task<bool> UpdateAsync(PlanEstudioDTO dto)
        {
            var plan = await _context.PlanEstudios.FindAsync(dto.Id);
            if (plan == null) return false;

            plan.Nombre = dto.Nombre;
            plan.IdCarrera = dto.IdCarrera;
            plan.AnioInicio = dto.AnioInicio;
            plan.EsVigente = dto.EsVigente;
            plan.ResolucionMinisterial = dto.ResolucionMinisterial;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var plan = await _context.PlanEstudios.FindAsync(id);
            if (plan == null) return false;

            plan.EsVigente = false; // Baja lógica por seguridad de integridad referencial
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PlanMateriaDTO>> GetMateriasByPlanAsync(int idPlan)
        {
            var materias = await _context.PlanMateria
                .AsNoTracking()
                .Include(pm => pm.IdMateriaNavigation)
                .Include(pm => pm.IdRegimenNavigation)
                .Where(pm => pm.IdPlan == idPlan)
                .Select(pm => new PlanMateriaDTO
                {
                    Id = pm.Id,
                    IdPlan = pm.IdPlan,
                    IdMateria = pm.IdMateria,
                    NombreMateria = pm.IdMateriaNavigation != null ? pm.IdMateriaNavigation.Nombre : "Sin Nombre",
                    CodigoMateria = pm.IdMateriaNavigation != null ? pm.IdMateriaNavigation.Codigo : "-",
                    AnioCursada = pm.AnioCursada,
                    IdRegimen = pm.IdRegimen,
                    NombreRegimen = pm.IdRegimenNavigation != null ? pm.IdRegimenNavigation.Nombre : "N/A",
                    Cuatrimestre = pm.Cuatrimestre,
                    CargaHorariaTotal = pm.CargaHorariaTotal,
                    EsLibre = pm.EsLibre,
                    TipoCalificacion = pm.TipoCalificacion ?? 0,
                    NotaMinimaRegularizar = pm.NotaMinimaRegularizar,
                    NotaMinimaAprobacion = pm.NotaMinimaAprobacion,
                    EsPromocionable = pm.EsPromocionable ?? false,
                    NotaMinimaPromocion = pm.NotaMinimaPromocion,
                    PorcentajeAsistenciaPromocion = pm.PorcentajeAsistenciaPromocion,
                    PorcentajeAsistenciaRegularizar = pm.PorcentajeAsistenciaRegularizar,
                    VigenciaCursadaAnios = pm.VigenciaCursadaAnios ?? 3,
                    TieneFinalObligatorio = pm.TieneFinalObligatorio ?? false,
                    TieneProyecto = pm.TieneProyecto ?? false,
                    CondicionesCursada = pm.CondicionesCursada,
                    CondicionesAprobacion = pm.CondicionesAprobacion,
                    Objetivos = pm.Objetivos,
                    ContenidosMinimos = pm.ContenidosMinimos,
                    DescripcionProyecto = pm.DescripcionProyecto,
                    CantidadParciales = pm.CantidadParciales ?? 2,
                    ModoAprobacionCursada = pm.ModoAprobacionCursada,
                    NotaEliminatoria = pm.NotaEliminatoria,
                    PromedioMinimoAprobacion = pm.PromedioMinimoAprobacion,
                    ModoNotaRecuperatorio = pm.ModoNotaRecuperatorio,
                    TieneIntegrador = pm.TieneIntegrador,
                    CondicionIntegradorParciales = pm.CondicionIntegradorParciales,
                    NotaAprobacionIntegrador = pm.NotaAprobacionIntegrador,
                    IntegradorPermitePromocion = pm.IntegradorPermitePromocion,
                    NotaPromocionIntegrador = pm.NotaPromocionIntegrador,
                    IdEstadoPromocion = pm.IdEstadoPromocion,
                    IdEstadoRegular = pm.IdEstadoRegular,
                    IdEstadoSiDesaprueba = pm.IdEstadoSiDesaprueba,
                    IdEstadoSiFaltaAsistencia = pm.IdEstadoSiFaltaAsistencia
                })
                .OrderBy(m => m.AnioCursada)
                .ToListAsync();

            if (materias.Any())
            {
                var idsPlanMateria = materias.Select(m => m.Id).ToList();

                // ✅ Sin Include: traemos solo los datos escalares que necesitamos
                var correlativas = await _context.Correlatividads
                    .AsNoTracking()
                    .Where(c => idsPlanMateria.Contains(c.IdPlanMateriaOrigen))
                    .Select(c => new
                    {
                        c.IdPlanMateriaOrigen,
                        c.IdPlanMateriaRequisito,
                        c.TipoRequisito,
                        NombreMateria = c.IdPlanMateriaRequisitoNavigation
                                           .IdMateriaNavigation.Nombre
                    })
                    .ToListAsync();

                foreach (var m in materias)
                {
                    var misReglas = correlativas
                        .Where(c => c.IdPlanMateriaOrigen == m.Id).ToList();

                    m.CorrelativasDetalle = misReglas.Select(c => new CorrelativaItemDTO
                    {
                        IdPlanMateriaRequisito = c.IdPlanMateriaRequisito,
                        TipoRequisito = c.TipoRequisito
                    }).ToList();

                    m.IdsCorrelativas = misReglas
                        .Select(c => c.IdPlanMateriaRequisito).ToList();

                    if (misReglas.Any())
                    {
                        m.CorrelativasTexto = string.Join(", ", misReglas
                            .Select(c => c.NombreMateria ?? "Materia"));
                    }
                }
            }

            return materias;
        }
        public async Task<bool> AgregarMateriaAsync(PlanMateria planMateria)
        {
            bool existe = await _context.PlanMateria
                .AsNoTracking()
                .AnyAsync(pm => pm.IdPlan == planMateria.IdPlan && pm.IdMateria == planMateria.IdMateria);

            if (existe) return false;

            _context.PlanMateria.Add(planMateria);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ModificarMateriaDelPlanAsync(PlanMateria pm)
        {
            var existente = await _context.PlanMateria.FindAsync(pm.Id);
            if (existente == null) return false;

            // Actualización inteligente: solo se mapean los campos necesarios
            _context.Entry(existente).CurrentValues.SetValues(pm);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> QuitarMateriaAsync(int idPlanMateria)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var item = await _context.PlanMateria.FindAsync(idPlanMateria);
                if (item == null) return false;

                // Borramos dependencias de correlatividad
                var reglas = await _context.Correlatividads
                    .Where(c => c.IdPlanMateriaOrigen == idPlanMateria || c.IdPlanMateriaRequisito == idPlanMateria)
                    .ToListAsync();

                _context.Correlatividads.RemoveRange(reglas);
                _context.PlanMateria.Remove(item);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> ActualizarCorrelativasAsync(int idPlanMateria, List<CorrelativaItemDTO> correlativas)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 🚀 FIX: Borrado directo en BD sin cargar en memoria (evita conflictos de tracking)
                await _context.Correlatividads
                    .Where(c => c.IdPlanMateriaOrigen == idPlanMateria)
                    .ExecuteDeleteAsync();

                // Insertamos las nuevas reglas
                if (correlativas != null && correlativas.Any())
                {
                    var nuevasReglas = correlativas.Select(item => new Correlatividad
                    {
                        IdPlanMateriaOrigen = idPlanMateria,
                        IdPlanMateriaRequisito = item.IdPlanMateriaRequisito,
                        TipoRequisito = item.TipoRequisito
                    }).ToList();

                    await _context.Correlatividads.AddRangeAsync(nuevasReglas);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<PlanMateria>> GetAllMateriasGlobalAsync()
        {
            return await _context.PlanMateria
                .AsNoTracking()
                .Include(pm => pm.IdMateriaNavigation)
                .Include(pm => pm.IdPlanNavigation)
                .OrderBy(pm => pm.IdMateriaNavigation.Nombre)
                .ToListAsync();
        }

        public async Task<List<PlanMateriaDTO>> GetMateriasPorSedeAsync(int idCarrera, int idSede)
        {
            var planesHabilitadosIds = await _context.PlanEstudioSedes
                .Where(ps => ps.IdSede == idSede &&
                             ps.IdPlanNavigation.IdCarrera == idCarrera &&
                             ps.Activo)
                .Select(ps => ps.IdPlan)
                .ToListAsync();

            if (!planesHabilitadosIds.Any())
                return new List<PlanMateriaDTO>();

            var materias = await _context.PlanMateria
                .Include(pm => pm.IdMateriaNavigation)
                .Include(pm => pm.IdPlanNavigation)
                .Where(pm => planesHabilitadosIds.Contains(pm.IdPlan))
                .OrderBy(pm => pm.AnioCursada)
                .ThenBy(pm => pm.IdMateriaNavigation.Nombre)
                .ToListAsync();

            return materias.Select(pm => new PlanMateriaDTO
            {
                Id = pm.Id,
                IdMateria = pm.IdMateria,
                NombreMateria = pm.IdMateriaNavigation.Nombre,
                AnioCursada = pm.AnioCursada,
                IdPlan = pm.IdPlan
            }).ToList();
        }

        public async Task<List<PlanSedeDTO>> GetSedesByPlanAsync(int idPlan)
        {
            // Obtenemos todas las sedes activas
            var sedes = await _context.Sedes.Where(s => s.Activo == true).ToListAsync();

            // Obtenemos las asignaciones actuales
            var asignaciones = await _context.PlanEstudioSedes
                .Where(ps => ps.IdPlan == idPlan && ps.Activo)
                .Select(ps => ps.IdSede)
                .ToListAsync();

            return sedes.Select(s => new PlanSedeDTO
            {
                IdPlan = idPlan,
                IdSede = s.Id,
                NombreSede = s.Nombre,
                Seleccionado = asignaciones.Contains(s.Id)
            }).ToList();
        }

        public async Task<bool> ActualizarSedesAsync(int idPlan, List<int> idsSedes)
        {
            var actuales = await _context.PlanEstudioSedes.Where(ps => ps.IdPlan == idPlan).ToListAsync();
            _context.PlanEstudioSedes.RemoveRange(actuales);

            foreach (var idSede in idsSedes)
            {
                _context.PlanEstudioSedes.Add(new PlanEstudioSede { IdPlan = idPlan, IdSede = idSede, Activo = true });
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}