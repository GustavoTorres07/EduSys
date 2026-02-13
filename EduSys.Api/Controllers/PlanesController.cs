using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using EduSys.Api.Data; // Necesario para acceder a PlanEstudioSede directamente si no está en el repo
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlanesController : ControllerBase
    {
        private readonly IPlanEstudioRepository _repo;
        private readonly EduSysDbContext _context; // Inyectamos el contexto para la consulta específica de Sedes

        public PlanesController(IPlanEstudioRepository repo, EduSysDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        // --- ENDPOINTS EXISTENTES (SIN CAMBIOS) ---

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var planes = await _repo.GetAllAsync();
            var dtos = planes.Select(p => new PlanEstudioDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                IdCarrera = p.IdCarrera,
                NombreCarrera = p.IdCarreraNavigation?.Nombre ?? "Sin Carrera",
                AnioInicio = p.AnioInicio,
                ResolucionMinisterial = p.ResolucionMinisterial,
                EsVigente = p.EsVigente ?? true,
                CantidadMaterias = p.PlanMateria.Count
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return NotFound();

            var dto = new PlanEstudioDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                IdCarrera = p.IdCarrera,
                NombreCarrera = p.IdCarreraNavigation?.Nombre,
                AnioInicio = p.AnioInicio,
                ResolucionMinisterial = p.ResolucionMinisterial,
                EsVigente = p.EsVigente ?? true
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PlanEstudioDTO dto)
        {
            var nuevo = new PlanEstudio
            {
                Nombre = dto.Nombre,
                IdCarrera = dto.IdCarrera,
                AnioInicio = dto.AnioInicio,
                ResolucionMinisterial = dto.ResolucionMinisterial,
                EsVigente = true
            };

            await _repo.CreateAsync(nuevo);
            return Ok(nuevo.Id);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] PlanEstudioDTO dto)
        {
            var plan = new PlanEstudio
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                ResolucionMinisterial = dto.ResolucionMinisterial,
                EsVigente = dto.EsVigente
            };

            if (!await _repo.UpdateAsync(plan)) return NotFound();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _repo.DeleteAsync(id)) return NotFound();
            return NoContent();
        }

        // --- MATERIAS DEL PLAN (EXISTENTE) ---

        [HttpGet("{idPlan}/materias")]
        public async Task<IActionResult> GetMaterias(int idPlan)
        {
            var lista = await _repo.GetMateriasByPlanAsync(idPlan);
            // ... (Tu mapeo existente se mantiene igual) ...
            var dtos = lista.Select(pm => new PlanMateriaDTO
            {
                Id = pm.Id,
                IdPlan = pm.IdPlan,
                IdMateria = pm.IdMateria,
                NombreMateria = pm.IdMateriaNavigation?.Nombre ?? "Desconocida",
                CodigoMateria = pm.IdMateriaNavigation?.Codigo,
                IdRegimen = pm.IdRegimen,
                NombreRegimen = pm.IdRegimenNavigation?.Nombre ?? "-",
                AnioCursada = pm.AnioCursada,
                Cuatrimestre = pm.Cuatrimestre,
                CargaHorariaTotal = pm.CargaHorariaTotal,
                TipoCalificacion = pm.TipoCalificacion ?? 0,
                EsPromocionable = pm.EsPromocionable ?? false,
                TieneFinalObligatorio = pm.TieneFinalObligatorio ?? false,
                NotaMinimaRegularizar = pm.NotaMinimaRegularizar ?? 4,
                NotaMinimaPromocion = pm.NotaMinimaPromocion,
                NotaMinimaAprobacion = pm.NotaMinimaAprobacion ?? 6,
                PorcentajeAsistenciaRegularizar = pm.PorcentajeAsistenciaRegularizar,
                PorcentajeAsistenciaPromocion = pm.PorcentajeAsistenciaPromocion,
                CantidadParciales = pm.CantidadParciales ?? 2,
                VigenciaCursadaAnios = pm.VigenciaCursadaAnios ?? 3,
                CondicionesCursada = pm.CondicionesCursada,
                CondicionesAprobacion = pm.CondicionesAprobacion,
                Objetivos = pm.Objetivos,
                ContenidosMinimos = pm.ContenidosMinimos,
                EsLibre = pm.EsLibre,
                TieneProyecto = pm.TieneProyecto ?? false,
                DescripcionProyecto = pm.DescripcionProyecto,
                IdsCorrelativas = pm.CorrelatividadIdPlanMateriaOrigenNavigations.Select(c => c.IdPlanMateriaRequisito).ToList(),
                CorrelativasTexto = string.Join(", ", pm.CorrelatividadIdPlanMateriaOrigenNavigations.Select(c => c.IdPlanMateriaRequisitoNavigation.IdMateriaNavigation.Nombre))
            }).ToList();

            return Ok(dtos);
        }

        // ==============================================================================
        // ✅ NUEVO ENDPOINT: OBTENER MATERIAS FILTRADAS POR SEDE Y CARRERA
        // Este es el que usarás en el Dropdown de "Nueva Comisión"
        // ==============================================================================
        [HttpGet("materias/carrera/{idCarrera}/sede/{idSede}")]
        public async Task<IActionResult> GetMateriasPorSede(int idCarrera, int idSede)
        {
            // 1. Buscar qué planes de esa carrera están habilitados en esa sede
            var planesHabilitadosIds = await _context.PlanEstudioSedes
                .Where(ps => ps.IdSede == idSede &&
                             ps.IdPlanNavigation.IdCarrera == idCarrera &&
                             ps.Activo)
                .Select(ps => ps.IdPlan)
                .ToListAsync();

            if (!planesHabilitadosIds.Any())
            {
                // Si no hay plan específico para la sede, opción de fallback:
                // Retornar vacío o buscar planes generales (depende de tu regla de negocio).
                // Por ahora retornamos vacío para obligar a configurar la sede.
                return Ok(new List<PlanMateriaDTO>());
            }

            // 2. Traer las materias de esos planes
            var materias = await _context.PlanMateria
                .Include(pm => pm.IdMateriaNavigation)
                .Include(pm => pm.IdPlanNavigation)
                .Where(pm => planesHabilitadosIds.Contains(pm.IdPlan))
                .OrderBy(pm => pm.AnioCursada)
                .ThenBy(pm => pm.IdMateriaNavigation.Nombre)
                .ToListAsync();

            // 3. Mapear a DTO (Simplificado para el combo)
            var dtos = materias.Select(pm => new PlanMateriaDTO
            {
                Id = pm.Id,
                IdMateria = pm.IdMateria,
                NombreMateria = pm.IdMateriaNavigation.Nombre,
                AnioCursada = pm.AnioCursada,
                IdPlan = pm.IdPlan,
                // Agregamos el nombre del plan por si hay varios vigentes
                // Ej: "Matemática (Plan 2026)"
                // Nota: Podrías agregar una propiedad "PlanNombre" en tu DTO si quieres mostrarlo
            }).ToList();

            return Ok(dtos);
        }

        // ... (Resto de métodos POST/PUT/DELETE de materias siguen igual) ...

        [HttpPost("materias")]
        public async Task<IActionResult> AgregarMateria([FromBody] PlanMateriaDTO dto)
        {
            var pm = new PlanMateria
            {
                IdPlan = dto.IdPlan,
                IdMateria = dto.IdMateria,
                AnioCursada = dto.AnioCursada,
                Cuatrimestre = dto.Cuatrimestre,
                IdRegimen = dto.IdRegimen,
                CargaHorariaTotal = dto.CargaHorariaTotal,
                TipoCalificacion = dto.TipoCalificacion,
                EsPromocionable = dto.EsPromocionable,
                TieneFinalObligatorio = dto.TieneFinalObligatorio,
                NotaMinimaRegularizar = dto.NotaMinimaRegularizar,
                NotaMinimaPromocion = dto.NotaMinimaPromocion,
                NotaMinimaAprobacion = dto.NotaMinimaAprobacion,
                PorcentajeAsistenciaRegularizar = dto.PorcentajeAsistenciaRegularizar,
                PorcentajeAsistenciaPromocion = dto.PorcentajeAsistenciaPromocion,
                CantidadParciales = dto.CantidadParciales,
                VigenciaCursadaAnios = dto.VigenciaCursadaAnios,
                CondicionesCursada = dto.CondicionesCursada,
                CondicionesAprobacion = dto.CondicionesAprobacion,
                Objetivos = dto.Objetivos,
                EsLibre = dto.EsLibre,
                ContenidosMinimos = dto.ContenidosMinimos,
                TieneProyecto = dto.TieneProyecto,
                DescripcionProyecto = dto.DescripcionProyecto
            };

            if (!await _repo.AgregarMateriaAsync(pm))
                return BadRequest("La materia ya existe en este plan.");

            return Ok();
        }

        [HttpDelete("materias/{idPlanMateria}")]
        public async Task<IActionResult> QuitarMateria(int idPlanMateria)
        {
            if (!await _repo.QuitarMateriaAsync(idPlanMateria)) return NotFound();
            return NoContent();
        }

        [HttpPost("materias/{idPlanMateria}/correlativas")]
        public async Task<IActionResult> ActualizarCorrelativas(int idPlanMateria, [FromBody] List<int> idsCorrelativas)
        {
            var exito = await _repo.ActualizarCorrelativasAsync(idPlanMateria, idsCorrelativas);
            if (!exito) return NotFound("La materia origen no existe.");
            return Ok();
        }

        [HttpPut("materias")]
        public async Task<IActionResult> EditarMateria([FromBody] PlanMateriaDTO dto)
        {
            var pm = new PlanMateria
            {
                Id = dto.Id,
                IdPlan = dto.IdPlan,
                IdMateria = dto.IdMateria,
                AnioCursada = dto.AnioCursada,
                Cuatrimestre = dto.Cuatrimestre,
                IdRegimen = dto.IdRegimen,
                CargaHorariaTotal = dto.CargaHorariaTotal,
                TipoCalificacion = dto.TipoCalificacion,
                NotaMinimaRegularizar = dto.NotaMinimaRegularizar,
                NotaMinimaAprobacion = dto.NotaMinimaAprobacion,
                EsPromocionable = dto.EsPromocionable,
                NotaMinimaPromocion = dto.NotaMinimaPromocion,
                PorcentajeAsistenciaRegularizar = dto.PorcentajeAsistenciaRegularizar,
                PorcentajeAsistenciaPromocion = dto.PorcentajeAsistenciaPromocion,
                CantidadParciales = dto.CantidadParciales,
                VigenciaCursadaAnios = dto.VigenciaCursadaAnios,
                TieneFinalObligatorio = dto.TieneFinalObligatorio,
                CondicionesCursada = dto.CondicionesCursada,
                CondicionesAprobacion = dto.CondicionesAprobacion,
                Objetivos = dto.Objetivos,
                EsLibre = dto.EsLibre,
                ContenidosMinimos = dto.ContenidosMinimos,
                TieneProyecto = dto.TieneProyecto,
                DescripcionProyecto = dto.DescripcionProyecto
            };

            if (await _repo.ModificarMateriaDelPlanAsync(pm))
                return Ok();

            return NotFound("No se encontró la materia en el plan.");
        }

        [HttpGet("materias/todas")]
        public async Task<IActionResult> GetAllMaterias()
        {
            var materias = await _repo.GetAllMateriasGlobalAsync();
            var dtos = materias.Select(pm => new PlanMateriaDTO
            {
                Id = pm.Id,
                IdMateria = pm.IdMateria,
                NombreMateria = pm.IdMateriaNavigation.Nombre,
                AnioCursada = pm.AnioCursada,
                IdPlan = pm.IdPlan
            });
            return Ok(dtos);
        }
    }
}