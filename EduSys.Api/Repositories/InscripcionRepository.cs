using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class InscripcionRepository : IInscripcionRepository
    {
        private readonly EduSysDbContext _context;

        public InscripcionRepository(EduSysDbContext context)
        {
            _context = context;
        }

        // ==============================================================================
        // 1. INSCRIBIR ALUMNO (Actualizado: recibe DTO completo y soporta EsLibre)
        // ==============================================================================
        public async Task<ResultadoInscripcionDTO> InscribirAlumnoAsync(InscripcionCursadaRequestDTO dto)
        {
            int idAlumno = dto.IdAlumno;
            int idComision = dto.IdComision;
            bool esLibre = dto.EsLibre;  // Viene del frontend (por defecto false si no se envía)

            // A. OBTENER DATOS DE LA COMISIÓN
            var comision = await _context.Comisions
                .Include(c => c.IdPeriodoNavigation)
                .Include(c => c.IdPlanMateriaNavigation)
                .FirstOrDefaultAsync(c => c.Id == idComision);

            if (comision == null) return Fail("La comisión no existe.");

            // B. VALIDAR EL PERIODO ACADÉMICO
            var periodo = comision.IdPeriodoNavigation;

            if (periodo.Estado != "Abierto" || (periodo.Activo.HasValue && !periodo.Activo.Value))
            {
                return Fail($"El periodo académico '{periodo.Nombre}' se encuentra cerrado administrativamente.");
            }

            var hoy = DateOnly.FromDateTime(DateTime.Now);
            if (hoy < periodo.FechaInicio || hoy > periodo.FechaFin)
            {
                return Fail($"Fuera de período de inscripción. Vigencia: {periodo.FechaInicio:dd/MM} al {periodo.FechaFin:dd/MM}.");
            }

            // C. VALIDAR ESTADO COMISIÓN
            if (comision.Estado != "Abierta") return Fail("La comisión no está habilitada para inscripciones.");

            // D. VERIFICAR SI YA ESTÁ INSCRIPTO
            var inscripcionExistente = await _context.InscripcionCursada
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(i => i.IdAlumno == idAlumno && i.IdComision == idComision);

            if (inscripcionExistente != null)
            {
                if (inscripcionExistente.Estado != "Baja")
                    return Fail("Ya estás inscripto en esta comisión.");
            }

            // E. VALIDAR UNICIDAD DE MATERIA (misma materia en otro turno/comisión/sede)
            var otraComisionMismaMateria = await _context.InscripcionCursada
                .Include(i => i.IdComisionNavigation)
                .AnyAsync(i => i.IdAlumno == idAlumno &&
                               i.IdComisionNavigation.IdPeriodo == comision.IdPeriodo &&
                               i.IdComisionNavigation.IdPlanMateria == comision.IdPlanMateria &&
                               i.IdComision != idComision &&
                               i.Estado != "Baja");

            if (otraComisionMismaMateria) return Fail("Ya te encuentras inscripto en otra comisión de esta misma materia.");

            // F. VALIDAR CORRELATIVAS
            var (cumpleCorrelativas, errorCorrelativa) = await ValidarCorrelativasDetalladoAsync(idAlumno, comision.IdPlanMateria);
            if (!cumpleCorrelativas) return Fail(errorCorrelativa);

            // G. VALIDAR CUPO DISPONIBLE
            var inscriptos = await _context.InscripcionCursada
                .CountAsync(i => i.IdComision == idComision && i.Estado != "Baja");

            if (inscriptos >= comision.CupoMaximo) return Fail("El cupo de la comisión está completo.");

            // --- GUARDAR O ACTUALIZAR ---
            try
            {
                if (inscripcionExistente != null && inscripcionExistente.Estado == "Baja")
                {
                    // Reactivar inscripción existente (Upsert)
                    inscripcionExistente.Estado = "Cursando";
                    inscripcionExistente.CondicionFinal = null;
                    inscripcionExistente.FechaInscripcion = DateTime.Now;
                    inscripcionExistente.EsLibre = esLibre;  // Respeta lo que venga del frontend
                }
                else
                {
                    // Crear nueva inscripción
                    var nueva = new InscripcionCursada
                    {
                        IdAlumno = idAlumno,
                        IdComision = idComision,
                        FechaInscripcion = DateTime.Now,
                        Estado = "Cursando",
                        CondicionFinal = null,
                        NotaFinalCursada = null,
                        EsLibre = esLibre  // Soporte para inscripción libre
                    };
                    _context.InscripcionCursada.Add(nueva);
                }

                await _context.SaveChangesAsync();
                return new ResultadoInscripcionDTO { Exito = true, Mensaje = "Inscripción realizada con éxito." };
            }
            catch (Exception ex)
            {
                return Fail("Error interno al procesar la inscripción: " + ex.Message);
            }
        }

        // ==============================================================================
        // 2. OBTENER OFERTA FILTRADA POR SEDE DEL ALUMNO
        // ==============================================================================
        public async Task<List<ComisionDTO>> GetOfertaParaAlumnoAsync(int idAlumno, int idPeriodo)
        {
            var alumno = await _context.Alumnos.FindAsync(idAlumno);
            if (alumno == null || alumno.IdPlanActual == null || alumno.IdSede == null)
                return new List<ComisionDTO>();

            var inscripcionesActivas = await _context.InscripcionCursada
                .Where(i => i.IdAlumno == idAlumno && i.Estado != "Baja" && i.IdComisionNavigation.IdPeriodo == idPeriodo)
                .Select(i => i.IdComision)
                .ToListAsync();

            var comisiones = await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(c => c.IdSedeNavigation)
                .Include(c => c.HorarioComisions)
                .Include(c => c.DocenteComisions).ThenInclude(dc => dc.IdDocenteNavigation).ThenInclude(d => d.IdUsuarioNavigation)
                .Where(c => c.IdPeriodo == idPeriodo
                            && c.Estado == "Abierta"
                            && c.IdPlanMateriaNavigation.IdPlan == alumno.IdPlanActual
                            && c.IdSede == alumno.IdSede)
                .ToListAsync();

            var oferta = new List<ComisionDTO>();

            foreach (var c in comisiones)
            {
                var (cumple, error) = await ValidarCorrelativasDetalladoAsync(idAlumno, c.IdPlanMateria);
                var inscriptos = await _context.InscripcionCursada.CountAsync(i => i.IdComision == c.Id && i.Estado != "Baja");

                var horariosTexto = string.Join(" / ", c.HorarioComisions
                    .OrderBy(h => h.DiaSemana)
                    .Select(h => $"{h.DiaSemana} {h.HoraInicio:hh\\:mm} hs - {h.HoraFin:hh\\:mm} hs"));
                if (string.IsNullOrEmpty(horariosTexto)) horariosTexto = "A confirmar";

                var docente = c.DocenteComisions.FirstOrDefault(dc => dc.Activo)?.IdDocenteNavigation?.IdUsuarioNavigation;
                string nombreProfesor = docente != null ? $"{docente.Apellido}, {docente.Nombre}" : "A designar";

                oferta.Add(new ComisionDTO
                {
                    Id = c.Id,
                    Codigo = c.Codigo,
                    MateriaNombre = c.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    SedeNombre = c.IdSedeNavigation.Nombre,
                    Turno = c.Turno,
                    Horarios = horariosTexto,
                    Profesor = nombreProfesor,
                    CupoMaximo = c.CupoMaximo,
                    AnioCursada = c.IdPlanMateriaNavigation.AnioCursada,
                    CupoDisponible = c.CupoMaximo - inscriptos,
                    CumpleCorrelativas = cumple,
                    EsMateriaLibre = c.IdPlanMateriaNavigation.EsLibre,
                    MensajeError = !cumple ? error : ((c.CupoMaximo - inscriptos) <= 0 ? "Cupo Completo" : null),
                    YaInscripto = inscripcionesActivas.Contains(c.Id)
                });
            }

            var ofertaSinDuplicados = oferta
                .GroupBy(x => new { x.MateriaNombre, x.Codigo, x.SedeNombre, x.Turno })
                .Select(g => g.First())
                .ToList();

            return ofertaSinDuplicados.OrderBy(o => o.AnioCursada).ThenBy(o => o.MateriaNombre).ToList();
        }

        // ==============================================================================
        // 3. CANCELAR INSCRIPCIÓN (BAJA)
        // ==============================================================================
        public async Task<bool> CancelarInscripcionAsync(int idInscripcion)
        {
            var item = await _context.InscripcionCursada
                .Include(i => i.IdComisionNavigation).ThenInclude(c => c.IdPeriodoNavigation)
                .FirstOrDefaultAsync(i => i.Id == idInscripcion);

            if (item == null) return false;

            var periodo = item.IdComisionNavigation.IdPeriodoNavigation;
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            if (hoy > periodo.FechaFin)
                throw new Exception("El periodo académico ha finalizado. No puedes darte de baja.");

            item.Estado = "Baja";
            await _context.SaveChangesAsync();
            return true;
        }

        // ==============================================================================
        // 4. MÉTODOS DE CONSULTA Y REPORTES
        // ==============================================================================

        public async Task<List<InscripcionCursada>> GetInscripcionesPorAlumnoAsync(int idAlumno, int idPeriodo)
        {
            return await _context.InscripcionCursada
                .Include(i => i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation)
                .Include(i => i.IdComisionNavigation.IdSedeNavigation)
                .Where(i => i.IdAlumno == idAlumno &&
                            i.IdComisionNavigation.IdPeriodo == idPeriodo &&
                            i.Estado != "Baja")
                .ToListAsync();
        }

        public async Task<List<InscripcionCursada>> GetInscripcionesPorComisionAsync(int idComision)
        {
            return await _context.InscripcionCursada
               .Where(i => i.IdComision == idComision && i.Estado != "Baja")
               .ToListAsync();
        }

        // ==============================================================================
        // 5. VALIDACIÓN DE CORRELATIVAS (con soporte para EsLibre)
        // ==============================================================================
        public async Task<bool> ValidarCorrelativasAsync(int idAlumno, int idPlanMateria)
        {
            var (cumple, _) = await ValidarCorrelativasDetalladoAsync(idAlumno, idPlanMateria);
            return cumple;
        }

        public async Task<ResultadoInscripcionDTO> InscribirAdminAsync(InscripcionManualDTO dto)
        {
            // 1. Obtener datos de la comisión y plan
            var comision = await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation)
                .Include(c => c.IdPeriodoNavigation)
                .FirstOrDefaultAsync(c => c.Id == dto.IdComision);

            if (comision == null)
                return new ResultadoInscripcionDTO { Exito = false, Mensaje = "La comisión no existe." };

            // 2. Validación CRÍTICA: Duplicados (Esta NUNCA se ignora para no romper la BD)
            bool yaInscripto = await _context.InscripcionCursada
                .AnyAsync(i => i.IdAlumno == dto.IdAlumno &&
                               i.IdComision == dto.IdComision &&
                               i.Estado != "Baja");

            if (yaInscripto)
                return new ResultadoInscripcionDTO { Exito = false, Mensaje = "El alumno ya está inscripto en esta materia." };

            // 3. Validaciones con OVERRIDE (Si el flag es false, validamos. Si es true, pasamos)

            // A. Cupo
            if (!dto.IgnorarCupo)
            {
                int inscriptos = await _context.InscripcionCursada
                    .CountAsync(i => i.IdComision == dto.IdComision && i.Estado != "Baja");

                if (inscriptos >= comision.CupoMaximo)
                    return new ResultadoInscripcionDTO { Exito = false, Mensaje = "El cupo está completo." };
            }

            // B. Correlativas (Usamos tu método existente de validación)
            if (!dto.IgnorarCorrelativas)
            {
                var (cumple, error) = await ValidarCorrelativasDetalladoAsync(dto.IdAlumno, comision.IdPlanMateria);
                if (!cumple)
                    return new ResultadoInscripcionDTO { Exito = false, Mensaje = $"Error de Correlativas: {error}" };
            }

            // C. Ventana de Fechas (Opcional, usualmente Admin opera fuera de fecha)
            if (!dto.IgnorarVentana)
            {
                if (comision.IdPeriodoNavigation.Estado != "Abierto")
                    return new ResultadoInscripcionDTO { Exito = false, Mensaje = "El periodo académico está cerrado." };
            }

            // 4. Guardar Inscripción Forzada
            try
            {
                var nuevaInscripcion = new InscripcionCursada
                {
                    IdAlumno = dto.IdAlumno,
                    IdComision = dto.IdComision,
                    FechaInscripcion = DateTime.Now,
                    Estado = "Cursando",
                    EsLibre = false // Podrías agregar este flag al DTO también si quieres
                };

                _context.InscripcionCursada.Add(nuevaInscripcion);
                await _context.SaveChangesAsync();

                return new ResultadoInscripcionDTO { Exito = true, Mensaje = "Inscripción administrativa realizada con éxito." };
            }
            catch (Exception ex)
            {
                return new ResultadoInscripcionDTO { Exito = false, Mensaje = $"Error interno: {ex.Message}" };
            }
        }

        private async Task<(bool, string)> ValidarCorrelativasDetalladoAsync(int idAlumno, int idPlanMateria)
        {
            var requisitos = await _context.Correlatividads
                .Include(c => c.IdPlanMateriaRequisitoNavigation)
                    .ThenInclude(pm => pm.IdMateriaNavigation)
                .Where(c => c.IdPlanMateriaOrigen == idPlanMateria)
                .ToListAsync();

            if (!requisitos.Any()) return (true, "");

            var faltantes = new List<string>();

            foreach (var req in requisitos)
            {
                bool cumple = false;
                int idMateriaReq = req.IdPlanMateriaRequisito;
                string nombreMateria = req.IdPlanMateriaRequisitoNavigation.IdMateriaNavigation.Nombre;

                // A. FINAL APROBADO (siempre cuenta, sea libre o no)
                var finalAprobado = await _context.InscripcionFinals
                    .Include(f => f.IdMesaFinalNavigation)
                    .AnyAsync(f => f.IdAlumno == idAlumno &&
                                   f.IdMesaFinalNavigation.IdPlanMateria == idMateriaReq &&
                                   f.Nota >= 4);

                if (finalAprobado)
                {
                    cumple = true;
                }
                // B. SOLO REGULAR → solo cuenta si NO es inscripción libre
                else if (req.TipoRequisito == "Regular")
                {
                    var tieneRegularValido = await _context.InscripcionCursada
                        .AnyAsync(i => i.IdAlumno == idAlumno &&
                                       i.IdComisionNavigation.IdPlanMateria == idMateriaReq &&
                                       i.Estado != "Baja" &&
                                       (i.CondicionFinal == "Regular" || i.CondicionFinal == "Promocionado") &&
                                       i.EsLibre == false);  // ← no cuenta si es libre

                    cumple = tieneRegularValido;
                }

                if (!cumple)
                {
                    faltantes.Add($"{nombreMateria} ({req.TipoRequisito})");
                }
            }

            if (faltantes.Any())
            {
                return (false, "Faltan correlativas: " + string.Join(", ", faltantes));
            }

            return (true, "");
        }

        public async Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesByAlumnoAsync(int idAlumno)
        {
            return await _context.InscripcionCursada
                .Include(i => i.IdComisionNavigation)
                    .ThenInclude(c => c.IdPlanMateriaNavigation)
                    .ThenInclude(pm => pm.IdMateriaNavigation)
                .Where(i => i.IdAlumno == idAlumno && i.Estado != "Baja")
                .Select(i => new InscripcionCursadaListadoDTO
                {
                    IdInscripcion = i.Id,
                    Materia = i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    ComisionCodigo = i.IdComisionNavigation.Codigo,
                    Estado = i.Estado,
                    Fecha = i.FechaInscripcion ?? DateTime.Now
                })
                .ToListAsync();
        }

        // Helper para respuestas fallidas
        private ResultadoInscripcionDTO Fail(string msg) => new ResultadoInscripcionDTO { Exito = false, Mensaje = msg };
    }
}