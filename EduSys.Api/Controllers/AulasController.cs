using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Protección añadida: Solo usuarios autenticados pueden ver las aulas
    public class AulasController : ControllerBase
    {
        // 🚀 Utilizamos el repositorio de Infraestructura que optimizamos anteriormente
        private readonly IInfrastructureRepository _repo;

        public AulasController(IInfrastructureRepository repo)
        {
            _repo = repo;
        }

        // GET: api/aulas/sede/{idSede}
        [HttpGet("sede/{idSede}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Aula>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Aula>>> GetBySede(int idSede)
        {
            // Llamamos al método correcto del IInfrastructureRepository
            var lista = await _repo.GetAulasBySedeAsync(idSede);
            return Ok(lista);
        }
    }
}