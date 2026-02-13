using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class CarreraRepository : ICarreraRepository
    {
        private readonly EduSysDbContext _context;

        public CarreraRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<Carrera>> GetAllAsync()
        {
            // Usamos INCLUDE para traer los datos relacionados (JOIN)
            return await _context.Carreras
                                 // Traemos Sedes
                                 .Include(c => c.CarreraSedes)
                                 .ThenInclude(cs => cs.IdSedeNavigation)
                                 // Traemos Modalidades (NUEVO)
                                 .Include(c => c.CarreraModalidads)
                                 .ThenInclude(cm => cm.IdModalidadNavigation)
                                 .ToListAsync();
        }

        public async Task<Carrera?> GetByIdAsync(int id)
        {
            // Cambiamos FindAsync por FirstOrDefaultAsync para poder usar INCLUDE
            return await _context.Carreras
                                 .Include(c => c.CarreraSedes)
                                 .ThenInclude(cs => cs.IdSedeNavigation)
                                 .Include(c => c.CarreraModalidads) // (NUEVO)
                                 .ThenInclude(cm => cm.IdModalidadNavigation)
                                 .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Carrera> CreateAsync(Carrera carrera)
        {
            _context.Carreras.Add(carrera);
            await _context.SaveChangesAsync();
            return carrera;
        }

        public async Task<bool> UpdateAsync(Carrera carrera)
        {
            var existe = await _context.Carreras.AnyAsync(c => c.Id == carrera.Id);
            if (!existe) return false;

            _context.Entry(carrera).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var carrera = await _context.Carreras.FindAsync(id);
            if (carrera == null) return false;

            carrera.Activo = false; // Baja lógica
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int idExcluir = 0)
        {
            return await _context.Carreras
                                 .AnyAsync(c => c.Nombre == nombre && c.Id != idExcluir);
        }

        // --- MÉTODOS PARA SEDES ---

        public async Task<List<int>> GetSedesIdsByCarreraAsync(int carreraId)
        {
            return await _context.CarreraSedes
                                 .Where(cs => cs.IdCarrera == carreraId && (cs.Activo ?? true))
                                 .Select(cs => cs.IdSede)
                                 .ToListAsync();
        }

        public async Task<bool> ActualizarSedesAsync(int carreraId, List<int> idsSedes)
        {
            var actuales = await _context.CarreraSedes
                                         .Where(cs => cs.IdCarrera == carreraId)
                                         .ToListAsync();

            _context.CarreraSedes.RemoveRange(actuales);

            foreach (var idSede in idsSedes)
            {
                _context.CarreraSedes.Add(new CarreraSede
                {
                    IdCarrera = carreraId,
                    IdSede = idSede,
                    Activo = true
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // --- MÉTODOS PARA MODALIDADES (NUEVOS) ---

        public async Task<List<int>> GetModalidadesIdsByCarreraAsync(int carreraId)
        {
            return await _context.CarreraModalidads
                                 .Where(cm => cm.IdCarrera == carreraId)
                                 .Select(cm => cm.IdModalidad)
                                 .ToListAsync();
        }

        public async Task<List<Carrera>> GetCarrerasPorSedeAsync(int idSede)
        {
            return await _context.CarreraSedes
                .Include(cs => cs.IdCarreraNavigation) // Traemos los datos de la carrera
                .Where(cs => cs.IdSede == idSede
                             && cs.Activo == true // Que la relación esté activa
                             && cs.IdCarreraNavigation.Activo == true) // Que la carrera esté activa
                .Select(cs => cs.IdCarreraNavigation) // Seleccionamos solo el objeto Carrera
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ActualizarModalidadesAsync(int carreraId, List<int> idsModalidades)
        {
            // 1. Buscamos las asociaciones viejas y las borramos
            var actuales = await _context.CarreraModalidads
                                         .Where(cm => cm.IdCarrera == carreraId)
                                         .ToListAsync();

            _context.CarreraModalidads.RemoveRange(actuales);

            // 2. Insertamos las nuevas
            foreach (var idMod in idsModalidades)
            {
                _context.CarreraModalidads.Add(new CarreraModalidad
                {
                    IdCarrera = carreraId,
                    IdModalidad = idMod
                    // La tabla intermedia CarreraModalidad usualmente no lleva 'Activo', se borra físicamente
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}