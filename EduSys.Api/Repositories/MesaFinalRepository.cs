using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class MesaFinalRepository : IMesaFinalRepository
    {
        private readonly EduSysDbContext _context;

        public MesaFinalRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<MesaFinalDTO>> GetAllAsync()
        {
            // Traemos todo con un buen Include para armar el DTO completo
            var mesas = await _context.MesaFinals
                .Include(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdPlanNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .Include(m => m.IdPeriodoNavigation)
                .Include(m => m.IdPresidenteMesaNavigation).ThenInclude(d => d.IdUsuarioNavigation)
                .Include(m => m.InscripcionFinals)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();

            return mesas.Select(MapearADTO).ToList();
        }

        public async Task<List<MesaFinalDTO>> GetByPeriodoAsync(int idPeriodo)
        {
            var mesas = await _context.MesaFinals
                .Include(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdPlanNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .Include(m => m.IdPeriodoNavigation)
                .Include(m => m.IdPresidenteMesaNavigation).ThenInclude(d => d.IdUsuarioNavigation)
                .Include(m => m.InscripcionFinals)
                .Where(m => m.IdPeriodo == idPeriodo)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();

            return mesas.Select(MapearADTO).ToList();
        }

        public async Task<MesaFinalDTO?> GetByIdAsync(int id)
        {
            var mesa = await _context.MesaFinals
                .Include(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(m => m.IdPeriodoNavigation)
                .Include(m => m.IdPresidenteMesaNavigation).ThenInclude(d => d.IdUsuarioNavigation)
                .Include(m => m.InscripcionFinals)
                .FirstOrDefaultAsync(m => m.Id == id);

            return mesa == null ? null : MapearADTO(mesa);
        }

        public async Task<ResultadoOperacionDTO> CreateAsync(MesaFinalRequestDTO dto)
        {
            try
            {
                var nueva = new MesaFinal
                {
                    IdPlanMateria = dto.IdPlanMateria,
                    IdPeriodo = dto.IdPeriodo,
                    IdPresidenteMesa = dto.IdPresidenteMesa,
                    IdVocal1 = dto.IdVocal1,
                    IdVocal2 = dto.IdVocal2,
                    FechaHora = dto.FechaHora,
                    Estado = "Abierta"
                };

                _context.MesaFinals.Add(nueva);
                await _context.SaveChangesAsync();
                return new ResultadoOperacionDTO { Exito = true, Mensaje = "Mesa final creada correctamente." };
            }
            catch (Exception ex)
            {
                return new ResultadoOperacionDTO { Exito = false, Mensaje = "Error al crear: " + ex.Message };
            }
        }

        public async Task<ResultadoOperacionDTO> UpdateAsync(MesaFinalRequestDTO dto)
        {
            var mesa = await _context.MesaFinals.FindAsync(dto.Id);
            if (mesa == null) return new ResultadoOperacionDTO { Exito = false, Mensaje = "No encontrada." };

            mesa.IdPlanMateria = dto.IdPlanMateria;
            mesa.IdPeriodo = dto.IdPeriodo;
            mesa.IdPresidenteMesa = dto.IdPresidenteMesa;
            mesa.IdVocal1 = dto.IdVocal1;
            mesa.IdVocal2 = dto.IdVocal2;
            mesa.FechaHora = dto.FechaHora;
            mesa.Estado = dto.Estado;

            await _context.SaveChangesAsync();
            return new ResultadoOperacionDTO { Exito = true, Mensaje = "Mesa final actualizada." };
        }

        public async Task<ResultadoOperacionDTO> DeleteAsync(int id)
        {
            var mesa = await _context.MesaFinals.Include(m => m.InscripcionFinals).FirstOrDefaultAsync(m => m.Id == id);
            if (mesa == null) return new ResultadoOperacionDTO { Exito = false, Mensaje = "No encontrada." };

            if (mesa.InscripcionFinals.Any())
                return new ResultadoOperacionDTO { Exito = false, Mensaje = "No se puede eliminar una mesa que ya tiene alumnos inscriptos." };

            _context.MesaFinals.Remove(mesa);
            await _context.SaveChangesAsync();
            return new ResultadoOperacionDTO { Exito = true, Mensaje = "Mesa eliminada." };
        }

        // Helper interno para mapear y no repetir código
        private MesaFinalDTO MapearADTO(MesaFinal m)
        {
            return new MesaFinalDTO
            {
                Id = m.Id,
                IdPlanMateria = m.IdPlanMateria,
                MateriaNombre = m.IdPlanMateriaNavigation?.IdMateriaNavigation?.Nombre ?? "S/D",
                CarreraNombre = m.IdPlanMateriaNavigation?.IdPlanNavigation?.IdCarreraNavigation?.Nombre ?? "S/D",
                IdPeriodo = m.IdPeriodo,
                PeriodoNombre = m.IdPeriodoNavigation?.Nombre ?? "S/D",
                IdPresidenteMesa = m.IdPresidenteMesa,
                PresidenteNombre = $"{m.IdPresidenteMesaNavigation?.IdUsuarioNavigation?.Apellido}, {m.IdPresidenteMesaNavigation?.IdUsuarioNavigation?.Nombre}",
                IdVocal1 = m.IdVocal1,
                IdVocal2 = m.IdVocal2,
                FechaHora = m.FechaHora,
                Estado = m.Estado,
                Libro = m.Libro,
                Folio = m.Folio,
                CantidadInscriptos = m.InscripcionFinals?.Count ?? 0
            };
        }
    }
}