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
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                    .ThenInclude(r => r.IdPermisos)
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            if (usuario == null) return null;

            bool claveCorrecta = BCrypt.Net.BCrypt.Verify(login.Clave, usuario.ClaveHash);
            if (!claveCorrecta) return null;

            string token = GenerarTokenJWT(usuario);

            // Validación segura de nulos por si el rol no tiene permisos asignados
            var listaPermisos = usuario.IdRolNavigation?.IdPermisos?.Select(p => p.Codigo).ToList() ?? new List<string>();

            bool esClaveInicial = BCrypt.Net.BCrypt.Verify(usuario.Dni, usuario.ClaveHash);

            return new SesionDTO
            {
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Rol = usuario.IdRolNavigation?.Nombre ?? "Sin Rol",
                Token = token,
                Permisos = listaPermisos,
                DebeCambiarPass = usuario.DebeCambiarPass || esClaveInicial, // Forzamos cambio si es la inicial
                FotoPerfilUrl = usuario.FotoPerfilUrl ?? string.Empty
            };
        }

        public async Task<Usuario> RegistrarAsync(Usuario usuario, string claveTextoPlano)
        {
            usuario.ClaveHash = BCrypt.Net.BCrypt.HashPassword(claveTextoPlano);
            usuario.FechaRegistro = DateTime.UtcNow; // 🚀 Mejor UTC para servidores
            usuario.Activo = true;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario> CrearAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            // 🚀 AsNoTracking para validaciones instantáneas
            return await _context.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> RestablecerClaveAsync(string email, string nuevaClaveHash)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return false;

            usuario.ClaveHash = nuevaClaveHash;
            usuario.DebeCambiarPass = true;

            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerarTokenJWT(Usuario usuario)
        {
            var key = _configuration.GetValue<string>("Jwt:Key");
            if (string.IsNullOrEmpty(key)) throw new InvalidOperationException("Falta configurar Jwt:Key");

            var keyBytes = Encoding.ASCII.GetBytes(key);
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()));

            if (!string.IsNullOrWhiteSpace(usuario.FotoPerfilUrl))
            {
                claims.Add(new Claim("FotoPerfilUrl", usuario.FotoPerfilUrl));
            }

            claims.Add(new Claim(ClaimTypes.Email, usuario.Email));
            claims.Add(new Claim(ClaimTypes.Name, $"{usuario.Apellido}, {usuario.Nombre}"));

            if (usuario.IdRolNavigation != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.IdRolNavigation.Nombre));

                if (usuario.IdRolNavigation.IdPermisos != null)
                {
                    foreach (var permiso in usuario.IdRolNavigation.IdPermisos)
                    {
                        claims.Add(new Claim("Permiso", permiso.Codigo));
                    }
                }
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
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(tokenConfig);
        }

        public async Task<bool> CambiarClaveDesdePerfilAsync(int idUsuario, string nuevaClaveHash)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return false;

            usuario.ClaveHash = nuevaClaveHash;
            usuario.DebeCambiarPass = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            // Traemos el usuario con su Rol y Nacionalidad (por si los necesitas)
            return await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Include(u => u.IdNacionalidadNavigation)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            var resultado = await _context.SaveChangesAsync();
            return resultado > 0;
        }
    }
}