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
    public class RolesController : ControllerBase
    {
        private readonly IRolRepository _rolRepo;

        public RolesController(IRolRepository rolRepo)
        {
            _rolRepo = rolRepo;
        }

        // GET: api/roles
        [HttpGet]
        // 🔒 CANDADO MIXTO: Quien gestiona Roles o quien gestiona Usuarios necesita ver esta lista
        [Authorize(Roles = "SEG_ROLES_ABM, SEG_USUARIOS_ABM")]
        public async Task<ActionResult<List<RolRequestDTO>>> GetRoles()
        {
            var roles = await _rolRepo.GetAllAsync();
            return Ok(roles);
        }

        // GET: api/roles/5
        [HttpGet("{id}")]
        // 🔒 CANDADO ESTRICTO: Solo gestión de Roles
        [Authorize(Roles = "SEG_ROLES_ABM")]
        public async Task<ActionResult<RolRequestDTO>> GetRol(int id)
        {
            var rol = await _rolRepo.GetByIdAsync(id);
            if (rol == null) return NotFound("El rol solicitado no existe.");

            return Ok(rol);
        }

        // GET: api/roles/permisos
        [HttpGet("permisos")]
        // 🔒 CANDADO ESTRICTO
        [Authorize(Roles = "SEG_ROLES_ABM")]
        public async Task<ActionResult<List<PermisoDTO>>> GetPermisos()
        {
            var permisos = await _rolRepo.GetPermisosCatalogoAsync();
            return Ok(permisos);
        }

        // POST: api/roles
        [HttpPost]
        // 🔒 CANDADO ESTRICTO
        [Authorize(Roles = "SEG_ROLES_ABM")]
        public async Task<IActionResult> GuardarRol([FromBody] RolRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre del rol es obligatorio.");

            var resultado = await _rolRepo.UpsertRolAsync(dto);

            if (resultado) return Ok(new { message = "Rol guardado correctamente." });

            return StatusCode(500, "Error al procesar la solicitud en la base de datos.");
        }

        // DELETE: api/roles/5
        [HttpDelete("{id}")]
        // 🔒 CANDADO ESTRICTO
        [Authorize(Roles = "SEG_ROLES_ABM")]
        public async Task<IActionResult> BajaRol(int id)
        {
            var resultado = await _rolRepo.BajaLogicaAsync(id);
            if (resultado) return Ok(new { message = "Rol desactivado correctamente." });

            return NotFound("No se pudo encontrar el rol para dar de baja.");
        }
    }
}