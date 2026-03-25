using EduSys.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔒 CANDADO REAL: Solo entra el Alumno (rol estructural) o el personal administrativo con llave ALU_ABM
    [Authorize(Roles = "Alumno, ALU_ABM")]
    public class HistorialAcademicoController : ControllerBase
    {
        private readonly IHistorialAcademicoRepository _repo;

        public HistorialAcademicoController(IHistorialAcademicoRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("avance/{idAlumno}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAvance(int idAlumno)
        {
            var result = await _repo.GetAvanceCarreraAsync(idAlumno);

            if (result == null)
                return NotFound(new { message = "Alumno no encontrado." });

            return Ok(result);
        }

        [HttpGet("cronologico/{idAlumno}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCronologico(int idAlumno)
        {
            var result = await _repo.GetHistorialCronologicoAsync(idAlumno);

            return Ok(result);
        }
    }
}