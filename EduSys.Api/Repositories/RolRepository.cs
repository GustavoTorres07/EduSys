using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories;

public class RolRepository : IRolRepository
{
    private readonly EduSysDbContext _context;

    public RolRepository(EduSysDbContext context) => _context = context;

    public async Task<List<RolRequestDTO>> GetAllAsync()
    {
        return await _context.Rols
            .AsNoTracking()
            .Select(r => new RolRequestDTO
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                Activo = r.Activo
            }).ToListAsync();
    }

    public async Task<RolRequestDTO?> GetByIdAsync(int id)
    {
        return await _context.Rols
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RolRequestDTO
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                Activo = r.Activo,
                IdsPermisos = r.IdPermisos.Select(p => p.Id).ToList()
            }).FirstOrDefaultAsync();
    }

    public async Task<bool> UpsertRolAsync(RolRequestDTO dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            Rol? rol;
            if (dto.Id == 0)
            {
                rol = new Rol { Nombre = dto.Nombre, Descripcion = dto.Descripcion, Activo = true };
                _context.Rols.Add(rol);
            }
            else
            {
                rol = await _context.Rols.Include(r => r.IdPermisos).FirstOrDefaultAsync(r => r.Id == dto.Id);
                if (rol == null) return false;
                rol.Nombre = dto.Nombre;
                rol.Descripcion = dto.Descripcion;
                rol.Activo = dto.Activo;
                rol.IdPermisos.Clear(); // Limpiamos para re-asignar
            }

            // Asignamos los nuevos permisos
            var permisos = await _context.Permisos.Where(p => dto.IdsPermisos.Contains(p.Id)).ToListAsync();
            foreach (var p in permisos) rol.IdPermisos.Add(p);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> BajaLogicaAsync(int id)
    {
        var rol = await _context.Rols.FindAsync(id);
        if (rol == null) return false;
        rol.Activo = false;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<PermisoDTO>> GetPermisosCatalogoAsync()
    {
        return await _context.Permisos
            .AsNoTracking()
            .Select(p => new PermisoDTO
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Descripcion = p.Descripcion,
                Modulo = p.Modulo
            }).ToListAsync();
    }
}