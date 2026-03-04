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

        // =======================================================================
        // LÓGICA PARA ACTAS DE EXÁMENES FINALES
        // =======================================================================

        public async Task<ActaMesaFinalDTO?> GetActaMesaFinalAsync(int idMesaFinal)
        {
            var mesa = await _context.MesaFinals
                .Include(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                // 👇 AQUÍ ESTÁ LA CORRECCIÓN: IdPlanNavigation en lugar de IdPlanEstudioNavigation 👇
                .Include(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdPlanNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .Include(m => m.IdPresidenteMesaNavigation).ThenInclude(d => d.IdUsuarioNavigation)
                .Include(m => m.InscripcionFinals)
                    .ThenInclude(i => i.IdAlumnoNavigation).ThenInclude(a => a.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.Id == idMesaFinal);

            if (mesa == null) return null;

            return new ActaMesaFinalDTO
            {
                IdMesaFinal = mesa.Id,
                MateriaNombre = mesa.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                // 👇 AQUÍ TAMBIÉN CORREGIDO 👇
                CarreraNombre = mesa.IdPlanMateriaNavigation.IdPlanNavigation.IdCarreraNavigation.Nombre,
                FechaHora = mesa.FechaHora,
                Tribunal = $"{mesa.IdPresidenteMesaNavigation.IdUsuarioNavigation.Apellido} (Presidente)",
                EstadoMesa = mesa.Estado ?? "Abierta",
                Libro = mesa.Libro ?? "",
                Folio = mesa.Folio ?? "",

                // Traemos a todos los inscriptos que no se hayan dado de baja
                Alumnos = mesa.InscripcionFinals.Where(i => i.Estado != "Baja").Select(i => new AlumnoActaFinalDTO
                {
                    IdInscripcion = i.Id,
                    IdAlumno = i.IdAlumno,
                    Legajo = i.IdAlumnoNavigation.Legajo,
                    AlumnoNombre = $"{i.IdAlumnoNavigation.IdUsuarioNavigation.Apellido}, {i.IdAlumnoNavigation.IdUsuarioNavigation.Nombre}",
                    Dni = i.IdAlumnoNavigation.IdUsuarioNavigation.Dni,
                    Condicion = "Regular",
                    Nota = i.Nota,
                    EstadoInscripcion = i.Estado ?? "Inscripto"
                }).OrderBy(a => a.AlumnoNombre).ToList()
            };
        }
        public async Task<bool> GuardarNotaFinalAsync(int idInscripcion, decimal? nota)
        {
            var inscripcion = await _context.InscripcionFinals.FindAsync(idInscripcion);
            if (inscripcion == null || inscripcion.Estado == "Baja") return false;

            inscripcion.Nota = nota;

            // Calculamos en vivo el estado al poner la nota
            if (nota == null) inscripcion.Estado = "Inscripto";
            else if (nota >= 4) inscripcion.Estado = "Aprobado";
            else inscripcion.Estado = "Reprobado";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CerrarActaFinalAsync(int idMesaFinal, string libro, string folio)
        {
            var mesa = await _context.MesaFinals
                .Include(m => m.InscripcionFinals)
                .FirstOrDefaultAsync(m => m.Id == idMesaFinal);

            if (mesa == null || mesa.Estado == "Cerrada") return false;

            // 1. Cerramos la mesa y guardamos libro/folio
            mesa.Estado = "Cerrada";
            mesa.Libro = libro;
            mesa.Folio = folio;

            // 2. Evaluamos a los alumnos que no tienen nota cargada y los pasamos a "Ausente"
            foreach (var inscripcion in mesa.InscripcionFinals.Where(i => i.Estado != "Baja"))
            {
                if (!inscripcion.Nota.HasValue)
                {
                    inscripcion.Estado = "Ausente";
                }
            }

            await _context.SaveChangesAsync();
            return true;
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