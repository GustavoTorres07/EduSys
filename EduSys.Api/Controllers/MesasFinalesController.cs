using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔓 Candado Base: Requiere autenticación
    [Authorize]
    public class MesasFinalesController : ControllerBase
    {
        private readonly IMesaFinalRepository _repo;

        public MesasFinalesController(IMesaFinalRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        // 🔒 CANDADO REAL: Ver todas las mesas (para el panel administrativo de ABM o Actas)
        [Authorize(Roles = "INS_FINAL_MESAS_ABM, FINAL_CARGAR_RESULTADO")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MesaFinalDTO>))]
        public async Task<ActionResult<List<MesaFinalDTO>>> Get()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpGet("periodo/{idPeriodo}")]
        // 🔓 LECTURA GENERAL: Hereda [Authorize]. 
        // Se deja abierto para que alumnos y docentes puedan ver la oferta de mesas disponibles.
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MesaFinalDTO>))]
        public async Task<ActionResult<List<MesaFinalDTO>>> GetByPeriodo(int idPeriodo)
        {
            return Ok(await _repo.GetByPeriodoAsync(idPeriodo));
        }

        [HttpPost]
        // 🔒 CANDADO REAL: Solo personal con permiso para crear/editar Mesas de Finales
        [Authorize(Roles = "INS_FINAL_MESAS_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoOperacionDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultadoOperacionDTO))]
        public async Task<ActionResult<ResultadoOperacionDTO>> Post([FromBody] MesaFinalRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repo.CreateAsync(dto);
            if (result.Exito) return Ok(result);
            return BadRequest(result);
        }

        [HttpPut]
        // 🔒 CANDADO REAL
        [Authorize(Roles = "INS_FINAL_MESAS_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoOperacionDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultadoOperacionDTO))]
        public async Task<ActionResult<ResultadoOperacionDTO>> Put([FromBody] MesaFinalRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _repo.UpdateAsync(dto);
            if (result.Exito) return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        // 🔒 CANDADO REAL
        [Authorize(Roles = "INS_FINAL_MESAS_ABM")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultadoOperacionDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultadoOperacionDTO))]
        public async Task<ActionResult<ResultadoOperacionDTO>> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (result.Exito) return Ok(result);
            return BadRequest(result);
        }

        // ==================================================================
        // ENDPOINTS DE CALIFICACIONES Y ACTAS
        // ==================================================================

        [HttpGet("{idMesa}/acta")]
        // 🔒 CANDADO MIXTO: Permite a los administrativos de actas o al Docente a cargo de la mesa
        [Authorize(Roles = "FINAL_CARGAR_RESULTADO, Docente")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActaMesaFinal(int idMesa)
        {
            var acta = await _repo.GetActaMesaFinalAsync(idMesa);
            if (acta == null)
                return NotFound(new { message = "La mesa no existe o no se pudo generar el acta." });

            return Ok(acta);
        }

        [HttpPut("inscripcion/{idInscripcion}/nota")]
        // 🔒 CANDADO MIXTO
        [Authorize(Roles = "FINAL_CARGAR_RESULTADO, Docente")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GuardarNotaFinal(int idInscripcion, [FromBody] decimal? nota)
        {
            var exito = await _repo.GuardarNotaFinalAsync(idInscripcion, nota);
            if (exito) return Ok(new { message = "Nota registrada correctamente." });

            return BadRequest(new { message = "No se pudo guardar la nota. Verifique si la mesa está abierta." });
        }

        [HttpPost("{idMesa}/cerrar-acta")]
        // 🔒 CANDADO MIXTO
        [Authorize(Roles = "FINAL_CARGAR_RESULTADO, Docente")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CerrarActaFinal(int idMesa, [FromBody] CierreActaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exito = await _repo.CerrarActaFinalAsync(idMesa, dto.Libro, dto.Folio);
            if (exito) return Ok(new { message = "Acta cerrada exitosamente." });

            return BadRequest(new { message = "No se pudo cerrar el acta. Verifica que todas las notas estén cargadas y el acta no esté ya cerrada." });
        }
    }
}