using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class InfrastructureRepository : IInfrastructureRepository
    {
        private readonly EduSysDbContext _context;
        public InfrastructureRepository(EduSysDbContext context) { _context = context; }

        // --- SEDES ---

        public async Task<List<Sede>> GetAllSedesAsync()
        {
            // 🚀 OPTIMIZADO: AsNoTracking. Traemos TODAS (sin filtrar Activo) 
            // para que el Frontend pueda filtrar o mostrar el historial.
            return await _context.Sedes
                .AsNoTracking()
                .Include(s => s.Aulas) // Incluimos aulas para el contador
                .OrderBy(s => s.Nombre)
                .ToListAsync();
        }

        public async Task<Sede?> GetSedeByIdAsync(int id)
        {
            // 🚀 OPTIMIZADO
            return await _context.Sedes
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> CreateSedeAsync(Sede sede)
        {
            _context.Sedes.Add(sede);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateSedeAsync(Sede sede)
        {
            var existing = await _context.Sedes.FindAsync(sede.Id);
            if (existing == null) return false;

            existing.Nombre = sede.Nombre;
            existing.Direccion = sede.Direccion;
            existing.CodigoPostal = sede.CodigoPostal;
            existing.Activo = sede.Activo;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteSedeAsync(int id)
        {
            var existing = await _context.Sedes.FindAsync(id);
            if (existing == null) return false;

            existing.Activo = false; // Baja Lógica
            return await _context.SaveChangesAsync() > 0;
        }

        // --- AULAS ---

        public async Task<List<Aula>> GetAulasBySedeAsync(int idSede)
        {
            // 🚀 OPTIMIZADO: AsNoTracking. Traemos también las inactivas.
            return await _context.Aulas
                .AsNoTracking()
                .Where(a => a.IdSede == idSede)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        public async Task<bool> CreateAulaAsync(Aula aula)
        {
            _context.Aulas.Add(aula);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAulaAsync(Aula aula)
        {
            var existing = await _context.Aulas.FindAsync(aula.Id);
            if (existing == null) return false;

            existing.Nombre = aula.Nombre;
            existing.Capacidad = aula.Capacidad;
            existing.Activo = aula.Activo;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAulaAsync(int id)
        {
            var existing = await _context.Aulas.FindAsync(id);
            if (existing == null) return false;

            existing.Activo = false; // Baja Lógica
            return await _context.SaveChangesAsync() > 0;
        }
    }
}