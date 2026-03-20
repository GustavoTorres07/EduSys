using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class ModalidadRepository : IModalidadRepository
    {
        private readonly EduSysDbContext _context;

        public ModalidadRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<Modalidad>> GetAllAsync()
        {
            // 🚀 OPTIMIZADO: AsNoTracking para velocidad y OrderBy para consistencia en la UI
            return await _context.Modalidads
                .AsNoTracking()
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<Modalidad?> GetByIdAsync(int id)
        {
            // 🚀 OPTIMIZADO: FirstOrDefaultAsync con AsNoTracking es preferible a FindAsync para lecturas puras
            return await _context.Modalidads
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Modalidad> CreateAsync(Modalidad modalidad)
        {
            _context.Modalidads.Add(modalidad);
            await _context.SaveChangesAsync();
            return modalidad;
        }

        public async Task<bool> UpdateAsync(Modalidad modalidad)
        {
            // Buscamos la entidad para que EF la rastree (Tracking)
            var existing = await _context.Modalidads.FirstOrDefaultAsync(m => m.Id == modalidad.Id);
            if (existing == null) return false;

            // 🚀 Actualizamos campos. EF detectará automáticamente qué cambió.
            existing.Nombre = modalidad.Nombre;
            existing.Codigo = modalidad.Codigo;
            existing.Activo = modalidad.Activo;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var modalidad = await _context.Modalidads.FindAsync(id);
            if (modalidad == null) return false;

            // Baja Lógica: Mantenemos la integridad referencial desactivando el registro
            modalidad.Activo = false;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int idExcluir = 0)
        {
            // 🚀 OPTIMIZADO: Comparación insensible a mayúsculas y AsNoTracking
            return await _context.Modalidads
                .AsNoTracking()
                .AnyAsync(m => m.Nombre.ToLower() == nombre.ToLower() && m.Id != idExcluir);
        }
    }
}