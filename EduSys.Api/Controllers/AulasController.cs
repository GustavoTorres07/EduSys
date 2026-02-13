using EduSys.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AulasController : ControllerBase
    {
        private readonly IAulaRepository _repo;

        public AulasController(IAulaRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("sede/{idSede}")]
        public async Task<IActionResult> GetBySede(int idSede)
        {
            var lista = await _repo.GetBySedeAsync(idSede);
            return Ok(lista);
        }
    }
}