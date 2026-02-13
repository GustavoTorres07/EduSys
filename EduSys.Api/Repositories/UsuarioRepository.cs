using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduSys.Api.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly EduSysDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuarioRepository(EduSysDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<SesionDTO> LoginAsync(LoginDTO login)
        {
            // 1. Buscamos el usuario por Email
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                    .ThenInclude(r => r.IdPermisos)
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            if (usuario == null) return null;

            // 2. Verificamos la contraseña
            bool claveCorrecta = BCrypt.Net.BCrypt.Verify(login.Clave, usuario.ClaveHash);
            if (!claveCorrecta) return null;

            // 3. Generamos Token
            string token = GenerarTokenJWT(usuario);
            var listaPermisos = usuario.IdRolNavigation.IdPermisos.Select(p => p.Codigo).ToList();

            // 4. LÓGICA DE CLAVE INICIAL (NUEVO)
            // Verificamos si la contraseña actual coincide con el hash del DNI.
            // Si da True, significa que nunca la cambió.
            bool esClaveInicial = BCrypt.Net.BCrypt.Verify(usuario.Dni, usuario.ClaveHash);

            // 5. Retornamos DTO completo
            return new SesionDTO
            {
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Rol = usuario.IdRolNavigation.Nombre,
                Token = token,
                Permisos = listaPermisos,
                DebeCambiarPass = usuario.DebeCambiarPass,
                FotoPerfilUrl = usuario.FotoPerfilUrl
            };
        }

        public async Task<Usuario> RegistrarAsync(Usuario usuario, string claveTextoPlano)
        {
            // Encriptamos la clave antes de guardarla
            usuario.ClaveHash = BCrypt.Net.BCrypt.HashPassword(claveTextoPlano);

            // Configuraciones por defecto
            usuario.FechaRegistro = DateTime.Now;
            usuario.Activo = true;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario> CrearAsync(Usuario usuario)
        {
            // NOTA: Asumimos que el usuario ya viene con la ClaveHash lista desde el Controlador
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Email == email);
        }
        private string GenerarTokenJWT(Usuario usuario)
        {
            var key = _configuration.GetValue<string>("Jwt:Key");
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var claims = new List<Claim>(); // Usamos List<Claim> en lugar de ClaimsIdentity directo para mayor control

            // 1. EL ID DE USUARIO (CRÍTICO PARA QUE FUNCIONE TODO)
            // Usamos usuario.Id.ToString() que es el entero (ej: "6")
            claims.Add(new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()));

            if (!string.IsNullOrWhiteSpace(usuario.FotoPerfilUrl))
            {
                claims.Add(new Claim("FotoPerfilUrl", usuario.FotoPerfilUrl));
            }

            // 2. Otros datos útiles
            claims.Add(new Claim(ClaimTypes.Email, usuario.Email));
            claims.Add(new Claim(ClaimTypes.Name, $"{usuario.Apellido}, {usuario.Nombre}"));
            claims.Add(new Claim(ClaimTypes.Role, usuario.IdRolNavigation.Nombre));

            // 3. Permisos
            foreach (var permiso in usuario.IdRolNavigation.IdPermisos)
            {
                claims.Add(new Claim("Permiso", permiso.Codigo));
            }

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature
            );

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = credenciales,
                Issuer = _configuration["Jwt:Issuer"],    // Asegúrate de incluir esto
                Audience = _configuration["Jwt:Audience"] // Y esto
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(tokenConfig);
        }

        public async Task<bool> RestablecerClaveAsync(string email, string nuevaClaveHash)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null) return false;

            usuario.ClaveHash = nuevaClaveHash;
            usuario.DebeCambiarPass = true; // Forzamos el cambio al entrar

            await _context.SaveChangesAsync();
            return true;
        }
    }
}