using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
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

        public async Task<List<PlanEstudio>> GetAllAsync()
        {
            return await _context.PlanEstudios
                .Include(p => p.IdCarreraNavigation)
                .Include(p => p.PlanMateria)
                .OrderByDescending(p => p.AnioInicio)
                .ToListAsync();
        }

        public async Task<PlanEstudio?> GetByIdAsync(int id)
        {
            return await _context.PlanEstudios
                .Include(p => p.IdCarreraNavigation)
                .Include(p => p.PlanMateria)
                .ThenInclude(pm => pm.IdMateriaNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PlanEstudio> CreateAsync(PlanEstudio plan)
        {
            _context.PlanEstudios.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<bool> UpdateAsync(PlanEstudio plan)
        {
            var existente = await _context.PlanEstudios.FindAsync(plan.Id);
            if (existente == null) return false;

            existente.Nombre = plan.Nombre;
            existente.ResolucionMinisterial = plan.ResolucionMinisterial;
            existente.EsVigente = plan.EsVigente;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var plan = await _context.PlanEstudios.FindAsync(id);
            if (plan == null) return false;

            plan.EsVigente = false; // Baja lógica
            await _context.SaveChangesAsync();
            return true;
        }

        // --- GESTIÓN DE MATERIAS DEL PLAN ---

        public async Task<List<PlanMateria>> GetMateriasByPlanAsync(int idPlan)
        {
            // Este método trae TODAS las columnas de la tabla PlanMateria,
            // incluyendo las nuevas (NotaMinimaRegularizar, etc.)
            return await _context.PlanMateria
                .Include(pm => pm.IdMateriaNavigation)
                .Include(pm => pm.IdRegimenNavigation)
                .Include(pm => pm.CorrelatividadIdPlanMateriaOrigenNavigations)
                    .ThenInclude(c => c.IdPlanMateriaRequisitoNavigation)
                        .ThenInclude(pmr => pmr.IdMateriaNavigation)
                .Where(pm => pm.IdPlan == idPlan)
                .OrderBy(pm => pm.AnioCursada)
                .ToListAsync();
        }

        public async Task<bool> AgregarMateriaAsync(PlanMateria planMateria)
        {
            // Validación básica: evitar duplicados
            bool existe = await _context.PlanMateria
                .AnyAsync(pm => pm.IdPlan == planMateria.IdPlan && pm.IdMateria == planMateria.IdMateria);

            if (existe) return false;

            // EF Core detecta automáticamente las nuevas propiedades del objeto planMateria
            _context.PlanMateria.Add(planMateria);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> QuitarMateriaAsync(int idPlanMateria)
        {
            var item = await _context.PlanMateria.FindAsync(idPlanMateria);
            if (item == null) return false;

            _context.PlanMateria.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarCorrelativasAsync(int idPlanMateriaOrigen, List<int> idsPlanMateriaRequisitos)
        {
            var planMateria = await _context.PlanMateria.FindAsync(idPlanMateriaOrigen);
            if (planMateria == null) return false;

            var actuales = await _context.Correlatividads
                .Where(c => c.IdPlanMateriaOrigen == idPlanMateriaOrigen)
                .ToListAsync();

            _context.Correlatividads.RemoveRange(actuales);

            if (idsPlanMateriaRequisitos != null && idsPlanMateriaRequisitos.Any())
            {
                foreach (var idRequisito in idsPlanMateriaRequisitos)
                {
                    if (idRequisito == idPlanMateriaOrigen) continue;

                    var nueva = new Correlatividad
                    {
                        IdPlanMateriaOrigen = idPlanMateriaOrigen,
                        IdPlanMateriaRequisito = idRequisito,
                        TipoRequisito = "Obligatoria" // Valor por defecto necesario
                    };
                    _context.Correlatividads.Add(nueva);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public async Task<bool> ModificarMateriaDelPlanAsync(PlanMateria pm)
        {
            var existente = await _context.PlanMateria.FindAsync(pm.Id);
            if (existente == null) return false;

            // Actualizamos los campos operativos
            existente.AnioCursada = pm.AnioCursada;
            existente.Cuatrimestre = pm.Cuatrimestre;
            existente.IdRegimen = pm.IdRegimen;
            existente.CargaHorariaTotal = pm.CargaHorariaTotal;
            existente.EsLibre = pm.EsLibre;
            // Actualizamos las Reglas Académicas (Nuevos campos)
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

            // Actualizamos Textos y Proyecto
            existente.Objetivos = pm.Objetivos;
            existente.ContenidosMinimos = pm.ContenidosMinimos;
            existente.CondicionesCursada = pm.CondicionesCursada;
            existente.CondicionesAprobacion = pm.CondicionesAprobacion;
            existente.TieneProyecto = pm.TieneProyecto;
            existente.DescripcionProyecto = pm.DescripcionProyecto;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception) { return false; }
        }

        public async Task<List<PlanMateria>> GetAllMateriasGlobalAsync()
        {
            return await _context.PlanMateria
                .Include(pm => pm.IdMateriaNavigation)
                .Include(pm => pm.IdPlanNavigation) // Para saber de qué plan es
                .OrderBy(pm => pm.IdMateriaNavigation.Nombre)
                .ToListAsync();
        }
    }
}