using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Lectura permitida para cualquier usuario logueado
    public class PlanesController : ControllerBase
    {
        private readonly IPlanEstudioRepository _repo;

        // ✅ INYECCIÓN LIMPIA: Se eliminó EduSysDbContext
        public PlanesController(IPlanEstudioRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PlanEstudioDTO>))]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _repo.GetAllAsync();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlanEstudioDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _repo.GetByIdAsync(id);
            if (dto == null) return NotFound(new { message = "Plan de estudio no encontrado." });
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] PlanEstudioDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newId = await _repo.CreateAsync(dto);
            return Ok(newId);
        }

        [HttpPut]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] PlanEstudioDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _repo.UpdateAsync(dto))
                return NotFound(new { message = "Plan de estudio no encontrado para actualizar." });

            return Ok(new { message = "Plan de estudio actualizado." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _repo.DeleteAsync(id))
                return NotFound(new { message = "Plan de estudio no encontrado." });

            return NoContent();
        }

        // ====================================================================
        // GESTIÓN DE MATERIAS DEL PLAN
        // ====================================================================

        [HttpGet("{idPlan}/materias")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PlanMateriaDTO>))]
        public async Task<IActionResult> GetMaterias(int idPlan)
        {
            var dtos = await _repo.GetMateriasByPlanAsync(idPlan);
            return Ok(dtos);
        }

        [HttpGet("materias/carrera/{idCarrera}/sede/{idSede}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PlanMateriaDTO>))]
        public async Task<IActionResult> GetMateriasPorSede(int idCarrera, int idSede)
        {
            // ✅ Delegamos la lógica pesada al repositorio
            var dtos = await _repo.GetMateriasPorSedeAsync(idCarrera, idSede);
            return Ok(dtos);
        }

        [HttpPost("materias")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AgregarMateria([FromBody] PlanMateriaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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

                ModoAprobacionCursada = dto.ModoAprobacionCursada,
                NotaEliminatoria = dto.NotaEliminatoria,
                PromedioMinimoAprobacion = dto.PromedioMinimoAprobacion,
                CantidadAplazosParaLibre = dto.CantidadAplazosParaLibre,
                IdEstadoPromocion = dto.IdEstadoPromocion,
                IdEstadoRegular = dto.IdEstadoRegular,
                IdEstadoSiDesaprueba = dto.IdEstadoSiDesaprueba,
                IdEstadoSiFaltaAsistencia = dto.IdEstadoSiFaltaAsistencia,

                ModoNotaRecuperatorio = dto.ModoNotaRecuperatorio,
                TieneIntegrador = dto.TieneIntegrador,
                CondicionIntegradorParciales = dto.CondicionIntegradorParciales,
                NotaAprobacionIntegrador = dto.NotaAprobacionIntegrador,
                IntegradorPermitePromocion = dto.IntegradorPermitePromocion,
                NotaPromocionIntegrador = dto.NotaPromocionIntegrador
            };

            if (!await _repo.AgregarMateriaAsync(pm))
                return BadRequest(new { message = "La materia ya existe en este plan." });

            return Ok(new { message = "Materia agregada al plan correctamente." });
        }

        [HttpPut("materias")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditarMateria([FromBody] PlanMateriaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var pm = new PlanMateria
            {
                Id = dto.Id,
                IdPlan = dto.IdPlan,
                IdMateria = dto.IdMateria,
                AnioCursada = dto.AnioCursada,
                Cuatrimestre = dto.Cuatrimestre,
                IdRegimen = dto.IdRegimen,
                // ... (Todos los demás campos se mapean igual que en el Create)
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

                ModoAprobacionCursada = dto.ModoAprobacionCursada,
                NotaEliminatoria = dto.NotaEliminatoria,
                PromedioMinimoAprobacion = dto.PromedioMinimoAprobacion,
                CantidadAplazosParaLibre = dto.CantidadAplazosParaLibre,
                IdEstadoPromocion = dto.IdEstadoPromocion,
                IdEstadoRegular = dto.IdEstadoRegular,
                IdEstadoSiDesaprueba = dto.IdEstadoSiDesaprueba,
                IdEstadoSiFaltaAsistencia = dto.IdEstadoSiFaltaAsistencia,

                ModoNotaRecuperatorio = dto.ModoNotaRecuperatorio,
                TieneIntegrador = dto.TieneIntegrador,
                CondicionIntegradorParciales = dto.CondicionIntegradorParciales,
                NotaAprobacionIntegrador = dto.NotaAprobacionIntegrador,
                IntegradorPermitePromocion = dto.IntegradorPermitePromocion,
                NotaPromocionIntegrador = dto.NotaPromocionIntegrador
            };

            if (await _repo.ModificarMateriaDelPlanAsync(pm))
                return Ok(new { message = "Reglas de la materia actualizadas correctamente." });

            return NotFound(new { message = "No se encontró la materia en el plan." });
        }

        [HttpDelete("materias/{idPlanMateria}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> QuitarMateria(int idPlanMateria)
        {
            if (!await _repo.QuitarMateriaAsync(idPlanMateria))
                return NotFound(new { message = "Materia no encontrada en el plan." });

            return NoContent();
        }

        [HttpPut("materias/{idPlanMateria}/correlativas")] 
        [Authorize(Roles = "Administrador, Secretaria Academica")] // 🔒
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ActualizarCorrelativas(int idPlanMateria, [FromBody] List<CorrelativaItemDTO> correlativas)
        {
            var exito = await _repo.ActualizarCorrelativasAsync(idPlanMateria, correlativas);

            if (exito) return Ok(new { message = "Correlativas guardadas exitosamente." });

            return BadRequest(new { message = "Error al guardar las correlativas." });
        }

        [HttpGet("materias/todas")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PlanMateriaDTO>))]
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
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{idPlan}/sedes")]
        public async Task<IActionResult> GetSedesDelPlan(int idPlan)
        {
            // Este método llama a la lógica que te pasé anteriormente en el Repositorio
            var dtos = await _repo.GetSedesByPlanAsync(idPlan);
            return Ok(dtos);
        }

        [HttpPut("{idPlan}/sedes")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<IActionResult> ActualizarSedesDelPlan(int idPlan, [FromBody] List<int> idsSedes)
        {
            var exito = await _repo.ActualizarSedesAsync(idPlan, idsSedes);
            if (exito) return Ok(new { message = "Sedes del plan actualizadas correctamente." });
            return BadRequest(new { message = "No se pudieron actualizar las sedes." });
        }
    }
}