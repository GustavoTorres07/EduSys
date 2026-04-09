using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActasController : ControllerBase
    {
        private readonly IActasRepository _actasRepo;

        public ActasController(IActasRepository actasRepo)
        {
            _actasRepo = actasRepo;
        }

        [HttpPost("cerrar-acta/{id}")]
        // 🚀 Agregamos Docente y Administrador
        [Authorize(Roles = "Docente, Administrador, EVA_CARGAR_NOTAS")]
        public async Task<IActionResult> CerrarActa(int id)
        {
            var exito = await _actasRepo.CerrarActaAsync(id);
            if (exito) return Ok(new { message = "Acta parcial cerrada y foliada automáticamente." });
            return BadRequest(new { message = "Error al cerrar el acta. Verifique que no esté ya cerrada." });
        }

        [HttpPost("cerrar-cursada/{idComision}")]
        // 🚀 Agregamos Docente y Administrador
        [Authorize(Roles = "Docente, Administrador, EVA_CARGAR_NOTAS")]
        public async Task<IActionResult> CerrarCursada(int idComision)
        {
            var exito = await _actasRepo.CerrarActaComisionAsync(idComision);
            if (exito) return Ok(new { message = "Cursada cerrada y actas individuales generadas." });
            return BadRequest(new { message = "Error al cerrar la cursada." });
        }

        [HttpPost("reabrir-acta/{id}")]
        // 🚀 Agregamos Docente y Administrador
        [Authorize(Roles = "Docente, Administrador, EVA_CARGAR_NOTAS, ACTA_VER")]
        public async Task<ActionResult> ReabrirActa(int id)
        {
            var exito = await _actasRepo.ReabrirActaAsync(id);
            if (!exito) return BadRequest(new { message = "No se pudo reabrir el acta." });
            return Ok(new { message = "Acta reabierta exitosamente." });
        }

        [HttpPost("comision/{idComision}/reabrir")]
        // 🚀 ¡AQUÍ ESTABA EL BLOQUEO! Agregamos Docente y Administrador
        [Authorize(Roles = "Docente, Administrador, EVA_CARGAR_NOTAS, ACTA_VER")]
        public async Task<IActionResult> ReabrirComision(int idComision)
        {
            var result = await _actasRepo.ReabrirActaComisionAsync(idComision);
            if (!result) return NotFound(new { message = "Comisión no encontrada." });
            return Ok(new { message = "Comisión reabierta exitosamente." });
        }

        [HttpPost("inscripcion/{id}/toggle-cierre")]
        // 🚀 Agregamos Docente y Administrador
        [Authorize(Roles = "Docente, Administrador, EVA_CARGAR_NOTAS, ACTA_VER")]
        public async Task<IActionResult> ToggleCierreIndividual(int id)
        {
            var result = await _actasRepo.ToggleCierreCursadaIndividualAsync(id);
            if (!result) return NotFound(new { message = "Inscripción no encontrada." });
            return Ok(new { message = "Estado de cierre alternado exitosamente." });
        }

        [HttpGet("historial-alumno/{idAlumno}")]
        [Authorize(Roles = "ACTA_VER, Administrador")]
        public async Task<ActionResult<List<ActaResumenDTO>>> GetActasPorAlumno(int idAlumno)
        {
            var actas = await _actasRepo.GetActasPorAlumnoAsync(idAlumno);
            return Ok(actas);
        }
    }
}