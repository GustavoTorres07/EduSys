using EduSys.Api.Repositories.Interfaces;
// using EduSys.Shared.DTOs; // Descomenta si usas el tipo fuerte en ProducesResponseType
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔒 Recomendación: Restringir acceso solo a personal administrativo
    // [Authorize(Roles = "Administrador, Secretaria Academica")] 
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _repo;

        public DashboardController(IDashboardRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        // 💡 Reemplaza 'object' por tu DTO real (ej. typeof(DashboardResumenDTO)) si lo tienes
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] // Útil si activas los roles
        public async Task<IActionResult> GetResumen()
        {
            var datos = await _repo.GetResumenAsync();
            return Ok(datos);
        }
    }
}