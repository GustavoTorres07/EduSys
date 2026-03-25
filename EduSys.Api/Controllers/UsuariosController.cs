using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 🔒 Protegemos este controlador para que solo Admins puedan crear/ver usuarios
    [Authorize(Roles = "Administrador, Secretaria Academica")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuariosController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        // POST: api/usuarios (Crear usuarios administrativos o docentes manualmente)
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Usuario))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear([FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Asumimos que la contraseña viene temporalmente en ClaveHash desde el frontend
            string claveTextoPlano = usuario.ClaveHash;

            if (string.IsNullOrEmpty(claveTextoPlano))
                return BadRequest(new { message = "La contraseña es obligatoria para registrar un nuevo usuario." });

            try
            {
                var nuevo = await _usuarioRepository.RegistrarAsync(usuario, claveTextoPlano);

                // 💡 Nota de seguridad: El repositorio ya debería estar devolviendo el objeto
                // sin la contraseña en texto plano, y con el Hash correctamente generado.
                return Ok(nuevo);
            }
            catch (Exception ex)
            {
                // Captura errores de negocio, ej: "El email ya existe"
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/usuarios/5 (Obtener perfil)
        [HttpGet("{id}")]
        [Authorize] // Permite a cualquier rol logueado ver su perfil
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioDTO))]
        public async Task<ActionResult<UsuarioDTO>> GetById(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            // Mapeamos a DTO para no enviar la contraseña ni datos sensibles
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

                // 🚀 MODIFICADO: Mapeamos las colecciones de roles en lugar de un solo rol
                IdRoles = usuario.IdRols.Select(r => r.Id).ToList(),
                NombresRoles = usuario.IdRols.Select(r => r.Nombre).ToList(),

                IdNacionalidad = usuario.IdNacionalidad,
                NombreNacionalidad = usuario.IdNacionalidadNavigation?.Nombre,
                Activo = usuario.Activo,
                FechaRegistro = usuario.FechaRegistro,
                FotoPerfilUrl = usuario.FotoPerfilUrl,
                DebeCambiarPass = usuario.DebeCambiarPass
            };

            return Ok(dto);
        }

        // PUT: api/usuarios/5 (Actualizar perfil)
        [HttpPut("{id}")]
        [Authorize] // Permite a cualquier rol logueado editar su perfil
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioDTO dto)
        {
            if (id != dto.Id) return BadRequest(new { message = "Los IDs no coinciden." });

            // 1. Buscamos el usuario real en la Base de Datos
            var usuarioBd = await _usuarioRepository.GetByIdAsync(id);
            if (usuarioBd == null) return NotFound(new { message = "Usuario no encontrado." });

            // 2. ACTUALIZAMOS SOLO LOS DATOS PERMITIDOS (Contacto)
            // No actualizamos DNI, Nombre ni Apellido por seguridad.
            usuarioBd.Email = dto.Email;
            usuarioBd.Telefono = dto.Telefono;
            usuarioBd.Direccion = dto.Direccion;
            usuarioBd.Localidad = dto.Localidad;

            // 3. Guardamos los cambios
            var exito = await _usuarioRepository.UpdateAsync(usuarioBd);

            if (!exito) return StatusCode(500, new { message = "No se pudieron guardar los cambios en la base de datos." });

            return NoContent(); // 204 No Content (Éxito sin devolver cuerpo)
        }
    }
}