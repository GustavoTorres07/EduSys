using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
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
        private readonly EduSysDbContext _context;

        public PlanesController(IPlanEstudioRepository repo, EduSysDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _repo.GetAllAsync();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _repo.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PlanEstudioDTO dto)
        {
            var newId = await _repo.CreateAsync(dto);
            return Ok(newId);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] PlanEstudioDTO dto)
        {
            if (!await _repo.UpdateAsync(dto)) return NotFound();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _repo.DeleteAsync(id)) return NotFound();
            return NoContent();
        }

        [HttpGet("{idPlan}/materias")]
        public async Task<IActionResult> GetMaterias(int idPlan)
        {
            var dtos = await _repo.GetMateriasByPlanAsync(idPlan);
            return Ok(dtos);
        }

        [HttpGet("materias/carrera/{idCarrera}/sede/{idSede}")]
        public async Task<IActionResult> GetMateriasPorSede(int idCarrera, int idSede)
        {
            var planesHabilitadosIds = await _context.PlanEstudioSedes
                .Where(ps => ps.IdSede == idSede &&
                             ps.IdPlanNavigation.IdCarrera == idCarrera &&
                             ps.Activo)
                .Select(ps => ps.IdPlan)
                .ToListAsync();

            if (!planesHabilitadosIds.Any())
            {
                return Ok(new List<PlanMateriaDTO>());
            }

            var materias = await _context.PlanMateria
                .Include(pm => pm.IdMateriaNavigation)
                .Include(pm => pm.IdPlanNavigation)
                .Where(pm => planesHabilitadosIds.Contains(pm.IdPlan))
                .OrderBy(pm => pm.AnioCursada)
                .ThenBy(pm => pm.IdMateriaNavigation.Nombre)
                .ToListAsync();

            var dtos = materias.Select(pm => new PlanMateriaDTO
            {
                Id = pm.Id,
                IdMateria = pm.IdMateria,
                NombreMateria = pm.IdMateriaNavigation.Nombre,
                AnioCursada = pm.AnioCursada,
                IdPlan = pm.IdPlan
            }).ToList();

            return Ok(dtos);
        }

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
                DescripcionProyecto = dto.DescripcionProyecto,

                // ✅ SE AGREGARON LOS NUEVOS CAMPOS AQUÍ
                ModoAprobacionCursada = dto.ModoAprobacionCursada,
                NotaEliminatoria = dto.NotaEliminatoria,
                PromedioMinimoAprobacion = dto.PromedioMinimoAprobacion,
                CantidadAplazosParaLibre = dto.CantidadAplazosParaLibre,
                IdEstadoPromocion = dto.IdEstadoPromocion,
                IdEstadoRegular = dto.IdEstadoRegular,
                IdEstadoSiDesaprueba = dto.IdEstadoSiDesaprueba,
                IdEstadoSiFaltaAsistencia = dto.IdEstadoSiFaltaAsistencia
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

        [HttpPut("materia/{idPlanMateria}/correlativas")]
        public async Task<ActionResult> ActualizarCorrelativas(int idPlanMateria, [FromBody] List<CorrelativaItemDTO> correlativas)
        {
            var exito = await _repo.ActualizarCorrelativasAsync(idPlanMateria, correlativas);
            if (exito) return Ok();
            return BadRequest("Error al guardar correlativas");
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
                DescripcionProyecto = dto.DescripcionProyecto,

                // ✅ SE AGREGARON LOS NUEVOS CAMPOS AQUÍ TAMBIÉN
                ModoAprobacionCursada = dto.ModoAprobacionCursada,
                NotaEliminatoria = dto.NotaEliminatoria,
                PromedioMinimoAprobacion = dto.PromedioMinimoAprobacion,
                CantidadAplazosParaLibre = dto.CantidadAplazosParaLibre,
                IdEstadoPromocion = dto.IdEstadoPromocion,
                IdEstadoRegular = dto.IdEstadoRegular,
                IdEstadoSiDesaprueba = dto.IdEstadoSiDesaprueba,
                IdEstadoSiFaltaAsistencia = dto.IdEstadoSiFaltaAsistencia
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