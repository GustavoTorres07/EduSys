using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔓 Candado Base
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuariosController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        // GET: api/usuarios
        [HttpGet]
        [Authorize(Roles = "SEG_USUARIOS_ABM")] // 🔒 Solo administradores de seguridad
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UsuarioDTO>))]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetAll()
        {
            // Nota: Podrías crear un método GetAllAsync en tu IUsuarioRepository, 
            // pero si no lo tienes, puedes usar el DbContext directamente si lo inyectas,
            // o idealmente, asegúrate de tener este método en tu Repositorio.
            var usuarios = await _usuarioRepository.GetAllAsync(); // Asegúrate de tener este método en tu Repo

            var dtos = usuarios.Select(u => new UsuarioDTO
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Dni = u.Dni,
                Email = u.Email,
                Activo = u.Activo,
                IdRoles = u.IdRols.Select(r => r.Id).ToList(),
                NombresRoles = u.IdRols.Select(r => r.Nombre).ToList()
            });

            return Ok(dtos);
        }

        // POST: api/usuarios (Crear usuario administrativo)
        [HttpPost]
        [Authorize(Roles = "SEG_USUARIOS_ABM")] // 🔒 Solo administradores
        public async Task<IActionResult> Crear([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string claveTextoPlano = usuario.ClaveHash;
            if (string.IsNullOrEmpty(claveTextoPlano))
                return BadRequest(new { message = "La contraseña es obligatoria para registrar un nuevo usuario." });

            try
            {
                var nuevo = await _usuarioRepository.RegistrarAsync(usuario, claveTextoPlano);
                return Ok(nuevo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/usuarios/5 (Obtener perfil)
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDTO>> GetById(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            var dto = new UsuarioDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Dni = usuario.Dni,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Direccion = usuario.Direccion,
                Localidad = usuario.Localidad,
                FechaNacimiento = usuario.FechaNacimiento,
                IdRoles = usuario.IdRols.Select(r => r.Id).ToList(),
                NombresRoles = usuario.IdRols.Select(r => r.Nombre).ToList(),
                Activo = usuario.Activo,
                FechaRegistro = usuario.FechaRegistro,
                FotoPerfilUrl = usuario.FotoPerfilUrl,
                DebeCambiarPass = usuario.DebeCambiarPass
            };

            return Ok(dto);
        }

        // PUT: api/usuarios/5/roles
        [HttpPut("{id}/roles")]
        [Authorize(Roles = "SEG_USUARIOS_ABM")] // 🔒 Solo administradores
        public async Task<IActionResult> UpdateRoles(int id, [FromBody] List<int> rolesIds)
        {
            // Llama a un método en tu repositorio para limpiar los roles actuales y asignar los nuevos
            var exito = await _usuarioRepository.ActualizarRolesAsync(id, rolesIds);
            if (!exito) return NotFound(new { message = "Usuario no encontrado o error al actualizar." });

            return Ok(new { message = "Roles actualizados correctamente." });
        }

        // PUT: api/usuarios/5 (Actualizar perfil contacto - abierto al dueño del perfil)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioDTO dto)
        {
            if (id != dto.Id) return BadRequest(new { message = "Los IDs no coinciden." });

            var usuarioBd = await _usuarioRepository.GetByIdAsync(id);
            if (usuarioBd == null) return NotFound(new { message = "Usuario no encontrado." });

            usuarioBd.Email = dto.Email;
            usuarioBd.Telefono = dto.Telefono;
            usuarioBd.Direccion = dto.Direccion;
            usuarioBd.Localidad = dto.Localidad;

            var exito = await _usuarioRepository.UpdateAsync(usuarioBd);
            if (!exito) return StatusCode(500, new { message = "No se pudieron guardar los cambios en la base de datos." });

            return NoContent();
        }
    }
}