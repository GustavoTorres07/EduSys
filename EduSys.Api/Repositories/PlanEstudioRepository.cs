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
            var planes = await _context.PlanEstudios
                .Include(p => p.IdCarreraNavigation)
                .Include(p => p.PlanMateria)
                .OrderByDescending(p => p.AnioInicio)
                .ToListAsync();

            return planes.Select(p => new PlanEstudioDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                IdCarrera = p.IdCarrera,
                NombreCarrera = p.IdCarreraNavigation?.Nombre ?? "Sin Carrera",
                AnioInicio = p.AnioInicio,
                EsVigente = p.EsVigente ?? false,
                ResolucionMinisterial = p.ResolucionMinisterial,
                CantidadMaterias = p.PlanMateria.Count
            }).ToList();
        }

        public async Task<PlanEstudioDTO?> GetByIdAsync(int id)
        {
            var p = await _context.PlanEstudios
                .Include(plan => plan.IdCarreraNavigation)
                .Include(plan => plan.PlanMateria)
                .FirstOrDefaultAsync(plan => plan.Id == id);

            if (p == null) return null;

            return new PlanEstudioDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                IdCarrera = p.IdCarrera,
                NombreCarrera = p.IdCarreraNavigation?.Nombre ?? "Sin Carrera",
                AnioInicio = p.AnioInicio,
                EsVigente = p.EsVigente ?? false,
                ResolucionMinisterial = p.ResolucionMinisterial,
                CantidadMaterias = p.PlanMateria.Count
            };
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

            plan.EsVigente = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PlanMateriaDTO>> GetMateriasByPlanAsync(int idPlan)
        {
            var materias = await _context.PlanMateria
                .Include(pm => pm.IdMateriaNavigation)
                .Include(pm => pm.IdRegimenNavigation)
                .Where(pm => pm.IdPlan == idPlan)
                .OrderBy(pm => pm.AnioCursada)
                .ToListAsync();

            var idsMaterias = materias.Select(m => m.Id).ToList();

            var correlativas = await _context.Correlatividads
                .Include(c => c.IdPlanMateriaRequisitoNavigation)
                    .ThenInclude(req => req.IdMateriaNavigation)
                .Where(c => idsMaterias.Contains(c.IdPlanMateriaOrigen))
                .ToListAsync();

            var resultado = new List<PlanMateriaDTO>();

            foreach (var pm in materias)
            {
                var dto = new PlanMateriaDTO
                {
                    Id = pm.Id,
                    IdPlan = pm.IdPlan,
                    IdMateria = pm.IdMateria,
                    NombreMateria = pm.IdMateriaNavigation.Nombre,
                    CodigoMateria = pm.IdMateriaNavigation.Codigo,
                    AnioCursada = pm.AnioCursada,
                    IdRegimen = pm.IdRegimen,
                    NombreRegimen = pm.IdRegimenNavigation?.Nombre,
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

                    // ✅ MAPEADO HACIA LA UI
                    ModoAprobacionCursada = pm.ModoAprobacionCursada,
                    NotaEliminatoria = pm.NotaEliminatoria,
                    PromedioMinimoAprobacion = pm.PromedioMinimoAprobacion,
                    CantidadAplazosParaLibre = pm.CantidadAplazosParaLibre,
                    IdEstadoPromocion = pm.IdEstadoPromocion,
                    IdEstadoRegular = pm.IdEstadoRegular,
                    IdEstadoSiDesaprueba = pm.IdEstadoSiDesaprueba,
                    IdEstadoSiFaltaAsistencia = pm.IdEstadoSiFaltaAsistencia
                };

                var misCorrelativas = correlativas.Where(c => c.IdPlanMateriaOrigen == pm.Id).ToList();

                dto.CorrelativasDetalle = misCorrelativas.Select(c => new CorrelativaItemDTO
                {
                    IdPlanMateriaRequisito = c.IdPlanMateriaRequisito,
                    TipoRequisito = c.TipoRequisito
                }).ToList();

                resultado.Add(dto);
            }

            return resultado;
        }

        public async Task<bool> AgregarMateriaAsync(PlanMateria planMateria)
        {
            bool existe = await _context.PlanMateria
                .AnyAsync(pm => pm.IdPlan == planMateria.IdPlan && pm.IdMateria == planMateria.IdMateria);

            if (existe) return false;

            _context.PlanMateria.Add(planMateria);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ModificarMateriaDelPlanAsync(PlanMateria pm)
        {
            var existente = await _context.PlanMateria.FindAsync(pm.Id);
            if (existente == null) return false;

            existente.AnioCursada = pm.AnioCursada;
            existente.Cuatrimestre = pm.Cuatrimestre;
            existente.IdRegimen = pm.IdRegimen;
            existente.CargaHorariaTotal = pm.CargaHorariaTotal;
            existente.EsLibre = pm.EsLibre;
            existente.TipoCalificacion = pm.TipoCalificacion;
            existente.NotaMinimaRegularizar = pm.NotaMinimaRegularizar;
            existente.NotaMinimaAprobacion = pm.NotaMinimaAprobacion;
            existente.EsPromocionable = pm.EsPromocionable;
            existente.NotaMinimaPromocion = pm.NotaMinimaPromocion;
            existente.PorcentajeAsistenciaRegularizar = pm.PorcentajeAsistenciaRegularizar;
            existente.PorcentajeAsistenciaPromocion = pm.PorcentajeAsistenciaPromocion;
            existente.CantidadParciales = pm.CantidadParciales;
            existente.VigenciaCursadaAnios = pm.VigenciaCursadaAnios;
            existente.TieneFinalObligatorio = pm.TieneFinalObligatorio;
            existente.Objetivos = pm.Objetivos;
            existente.ContenidosMinimos = pm.ContenidosMinimos;
            existente.CondicionesCursada = pm.CondicionesCursada;
            existente.CondicionesAprobacion = pm.CondicionesAprobacion;
            existente.TieneProyecto = pm.TieneProyecto;
            existente.DescripcionProyecto = pm.DescripcionProyecto;

            // ✅ MAPEADO HACIA LA BASE DE DATOS
            existente.ModoAprobacionCursada = pm.ModoAprobacionCursada;
            existente.NotaEliminatoria = pm.NotaEliminatoria;
            existente.PromedioMinimoAprobacion = pm.PromedioMinimoAprobacion;
            existente.CantidadAplazosParaLibre = pm.CantidadAplazosParaLibre;
            existente.IdEstadoPromocion = pm.IdEstadoPromocion;
            existente.IdEstadoRegular = pm.IdEstadoRegular;
            existente.IdEstadoSiDesaprueba = pm.IdEstadoSiDesaprueba;
            existente.IdEstadoSiFaltaAsistencia = pm.IdEstadoSiFaltaAsistencia;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception) { return false; }
        }

        public async Task<bool> QuitarMateriaAsync(int idPlanMateria)
        {
            var item = await _context.PlanMateria.FindAsync(idPlanMateria);
            if (item == null) return false;

            var reglas = _context.Correlatividads.Where(c => c.IdPlanMateriaOrigen == idPlanMateria || c.IdPlanMateriaRequisito == idPlanMateria);
            _context.Correlatividads.RemoveRange(reglas);

            _context.PlanMateria.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarCorrelativasAsync(int idPlanMateria, List<CorrelativaItemDTO> correlativas)
        {
            var viejas = _context.Correlatividads.Where(c => c.IdPlanMateriaOrigen == idPlanMateria);
            _context.Correlatividads.RemoveRange(viejas);

            foreach (var item in correlativas)
            {
                _context.Correlatividads.Add(new Correlatividad
                {
                    IdPlanMateriaOrigen = idPlanMateria,
                    IdPlanMateriaRequisito = item.IdPlanMateriaRequisito,
                    TipoRequisito = item.TipoRequisito
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PlanMateria>> GetAllMateriasGlobalAsync()
        {
            return await _context.PlanMateria
                .Include(pm => pm.IdMateriaNavigation)
                .Include(pm => pm.IdPlanNavigation)
                .OrderBy(pm => pm.IdMateriaNavigation.Nombre)
                .ToListAsync();
        }
    }
}