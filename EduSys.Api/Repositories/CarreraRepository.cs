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
            // 🚀 OPTIMIZADO: AsNoTracking
            return await _context.Carreras
                                 .AsNoTracking()
                                 .Include(c => c.CarreraSedes)
                                    .ThenInclude(cs => cs.IdSedeNavigation)
                                 .Include(c => c.CarreraModalidads)
                                    .ThenInclude(cm => cm.IdModalidadNavigation)
                                 .ToListAsync();
        }

        public async Task<Carrera?> GetByIdAsync(int id)
        {
            // 🚀 OPTIMIZADO: AsNoTracking (Solo lectura, el update tiene su propio tracking)
            return await _context.Carreras
                                 .AsNoTracking()
                                 .Include(c => c.CarreraSedes)
                                    .ThenInclude(cs => cs.IdSedeNavigation)
                                 .Include(c => c.CarreraModalidads)
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
            // 🚀 OPTIMIZADO: AsNoTracking y lógica más limpia
            return await _context.Carreras
                                 .AsNoTracking()
                                 .AnyAsync(c => c.Nombre.ToLower() == nombre.ToLower() && c.Id != idExcluir);
        }

        // --- MÉTODOS PARA SEDES ---

        public async Task<List<int>> GetSedesIdsByCarreraAsync(int carreraId)
        {
            // 🚀 OPTIMIZADO: AsNoTracking
            return await _context.CarreraSedes
                                 .AsNoTracking()
                                 .Where(cs => cs.IdCarrera == carreraId && cs.Activo == true)
                                 .Select(cs => cs.IdSede)
                                 .ToListAsync();
        }

        public async Task<bool> ActualizarSedesAsync(int carreraId, List<int> idsSedes)
        {
            // Este SÍ usa Tracking porque vamos a borrar las entidades
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

        public async Task<List<Carrera>> GetCarrerasPorSedeAsync(int idSede)
        {
            // 🚀 OPTIMIZADO: AsNoTracking
            return await _context.CarreraSedes
                .AsNoTracking()
                .Include(cs => cs.IdCarreraNavigation)
                .Where(cs => cs.IdSede == idSede
                             && cs.Activo == true
                             && cs.IdCarreraNavigation.Activo == true)
                .Select(cs => cs.IdCarreraNavigation)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        // --- MÉTODOS PARA MODALIDADES ---

        public async Task<List<int>> GetModalidadesIdsByCarreraAsync(int carreraId)
        {
            // 🚀 OPTIMIZADO: AsNoTracking
            return await _context.CarreraModalidads
                                 .AsNoTracking()
                                 .Where(cm => cm.IdCarrera == carreraId)
                                 .Select(cm => cm.IdModalidad)
                                 .ToListAsync();
        }

        public async Task<bool> ActualizarModalidadesAsync(int carreraId, List<int> idsModalidades)
        {
            // Este SÍ usa Tracking porque vamos a borrar las entidades
            var actuales = await _context.CarreraModalidads
                                         .Where(cm => cm.IdCarrera == carreraId)
                                         .ToListAsync();

            _context.CarreraModalidads.RemoveRange(actuales);

            foreach (var idMod in idsModalidades)
            {
                _context.CarreraModalidads.Add(new CarreraModalidad
                {
                    IdCarrera = carreraId,
                    IdModalidad = idMod
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}