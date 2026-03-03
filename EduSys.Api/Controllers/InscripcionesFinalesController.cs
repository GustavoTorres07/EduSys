using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere login (especialmente de Alumno)
    public class InscripcionesFinalesController : ControllerBase
    {
        private readonly IInscripcionFinalRepository _repo;

        public InscripcionesFinalesController(IInscripcionFinalRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("oferta/{idAlumno}")]
        public async Task<ActionResult<List<MesaFinalOfertaDTO>>> GetOferta(int idAlumno, [FromQuery] int idPeriodo)
        {
            return Ok(await _repo.GetOfertaParaAlumnoAsync(idAlumno, idPeriodo));
        }

        [HttpGet("mis-inscripciones/{idAlumno}")]
        public async Task<ActionResult<List<MesaFinalOfertaDTO>>> GetMisInscripciones(int idAlumno, [FromQuery] int idPeriodo)
        {
            return Ok(await _repo.GetMisInscripcionesAsync(idAlumno, idPeriodo));
        }

        [HttpPost("inscribir")]
        public async Task<ActionResult<ResultadoOperacionDTO>> Inscribir(InscripcionFinalRequestDTO dto)
        {
            var res = await _repo.InscribirAlumnoAsync(dto);
            if (res.Exito) return Ok(res);
            return BadRequest(res); // Rebota si está fuera de término o rompe reglas
        }

        [HttpDelete("cancelar/{idInscripcion}")]
        public async Task<ActionResult<ResultadoOperacionDTO>> Cancelar(int idInscripcion, [FromQuery] int idAlumno)
        {
            var res = await _repo.CancelarInscripcionAsync(idInscripcion, idAlumno);
            if (res.Exito) return Ok(res);
            return BadRequest(res);
        }
    }
}