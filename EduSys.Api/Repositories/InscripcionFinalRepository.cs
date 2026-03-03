using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class InscripcionFinalRepository : IInscripcionFinalRepository
    {
        private readonly EduSysDbContext _context;

        public InscripcionFinalRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<MesaFinalOfertaDTO>> GetOfertaParaAlumnoAsync(int idAlumno, int idPeriodo)
        {
            var alumno = await _context.Alumnos
                .Include(a => a.IdPlanActualNavigation).ThenInclude(p => p.PlanMateria)
                .FirstOrDefaultAsync(a => a.Id == idAlumno);

            if (alumno == null || alumno.IdPlanActualNavigation == null) return new List<MesaFinalOfertaDTO>();

            // 1. Obtener las mesas del periodo actual (solo de las materias de su plan)
            var idsMateriasPlan = alumno.IdPlanActualNavigation.PlanMateria.Select(pm => pm.Id).ToList();
            var mesas = await _context.MesaFinals
                .Include(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(m => m.IdPresidenteMesaNavigation).ThenInclude(d => d.IdUsuarioNavigation)
                .Where(m => m.IdPeriodo == idPeriodo && idsMateriasPlan.Contains(m.IdPlanMateria) && m.Estado == "Abierta")
                .ToListAsync();

            // 2. Calcular el historial leyendo Cursadas y Finales directamente (IGNORANDO BAJAS)
            var cursadas = await _context.InscripcionCursada
                .Include(c => c.IdComisionNavigation)
                .Where(c => c.IdAlumno == idAlumno && c.Estado != "Baja" && (c.CondicionFinal == "Regular" || c.CondicionFinal == "Promocionado" || c.CondicionFinal == "Aprobado"))
                .ToListAsync();

            var finalesAprobados = await _context.InscripcionFinals
                .Include(f => f.IdMesaFinalNavigation)
                .Where(f => f.IdAlumno == idAlumno && f.Estado == "Aprobado")
                .ToListAsync();

            // 3. Obtener inscripciones vigentes a finales en este periodo
            var misInscripciones = await _context.InscripcionFinals
                .Where(i => i.IdAlumno == idAlumno && i.IdMesaFinalNavigation.IdPeriodo == idPeriodo && i.Estado != "Baja")
                .ToListAsync();

            // 4. Traer TODAS las reglas correlativas de las materias del plan
            var reglasCorrelativas = await _context.Correlatividads
                .Include(c => c.IdPlanMateriaRequisitoNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Where(c => idsMateriasPlan.Contains(c.IdPlanMateriaOrigen))
                .ToListAsync();

            var oferta = new List<MesaFinalOfertaDTO>();

            foreach (var mesa in mesas)
            {
                var dto = new MesaFinalOfertaDTO
                {
                    IdMesaFinal = mesa.Id,
                    IdPlanMateria = mesa.IdPlanMateria,
                    MateriaNombre = mesa.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    AnioCursada = mesa.IdPlanMateriaNavigation.AnioCursada,
                    FechaHora = mesa.FechaHora,
                    Tribunal = $"{mesa.IdPresidenteMesaNavigation.IdUsuarioNavigation.Apellido} (Pres.)",
                    PuedeInscribirse = true
                };

                // A. Validar si ya está inscripto
                var inscripcion = misInscripciones.FirstOrDefault(i => i.IdMesaFinal == mesa.Id);
                if (inscripcion != null)
                {
                    dto.YaInscripto = true;
                    dto.IdInscripcionFinal = inscripcion.Id;
                    dto.Condicion = inscripcion.Estado ?? "Regular";
                    dto.PuedeInscribirse = false;
                    dto.MotivoBloqueo = "Ya estás inscripto a esta mesa.";
                    oferta.Add(dto);
                    continue;
                }

                // B. Validar Estado Académico de la materia actual (¿Tiene la Cursada aprobada?)
                bool estaAprobada = finalesAprobados.Any(f => f.IdMesaFinalNavigation.IdPlanMateria == mesa.IdPlanMateria) ||
                                    cursadas.Any(c => c.IdComisionNavigation.IdPlanMateria == mesa.IdPlanMateria && (c.CondicionFinal == "Promocionado" || c.CondicionFinal == "Aprobado"));

                bool estaRegular = cursadas.Any(c => c.IdComisionNavigation.IdPlanMateria == mesa.IdPlanMateria && c.CondicionFinal == "Regular");

                if (estaAprobada)
                {
                    dto.PuedeInscribirse = false;
                    dto.MotivoBloqueo = "Materia ya aprobada.";
                }
                else if (estaRegular)
                {
                    dto.Condicion = "Regular";
                }
                else if (mesa.IdPlanMateriaNavigation.EsLibre)
                {
                    dto.Condicion = "Libre";
                }
                else
                {
                    dto.PuedeInscribirse = false;
                    dto.MotivoBloqueo = "Debes tener la cursada aprobada (Regular) para rendir el final.";
                }

                // ====================================================================
                // C. VALIDAR CORRELATIVAS PARA RENDIR FINAL (NUEVO MOTOR)
                // ====================================================================
                if (dto.PuedeInscribirse)
                {
                    // Buscamos ÚNICAMENTE las reglas que el Secretario marcó "Para Rendir"
                    var requisitosParaRendir = reglasCorrelativas
                        .Where(r => r.IdPlanMateriaOrigen == mesa.IdPlanMateria && r.TipoRequisito.StartsWith("Rendir-"))
                        .ToList();

                    var faltantes = new List<string>();

                    foreach (var req in requisitosParaRendir)
                    {
                        // Dividimos el texto (Ej: "Rendir-Aprobada" -> "Aprobada")
                        var partes = req.TipoRequisito.Split('-');
                        string condicionExigida = partes.Length == 2 ? partes[1] : "Aprobada"; // Por defecto Aprobada

                        // ¿Cómo está el alumno en la materia que se le exige?
                        bool correlativaAprobada = finalesAprobados.Any(f => f.IdMesaFinalNavigation.IdPlanMateria == req.IdPlanMateriaRequisito) ||
                                                   cursadas.Any(c => c.IdComisionNavigation.IdPlanMateria == req.IdPlanMateriaRequisito && (c.CondicionFinal == "Promocionado" || c.CondicionFinal == "Aprobado"));

                        bool correlativaRegular = cursadas.Any(c => c.IdComisionNavigation.IdPlanMateria == req.IdPlanMateriaRequisito && c.CondicionFinal == "Regular");

                        bool cumpleRequisito = false;

                        // Evaluamos la regla
                        if (condicionExigida == "Regular")
                        {
                            cumpleRequisito = correlativaAprobada || correlativaRegular;
                        }
                        else // Si le exigieron "Aprobada" (que es el 99% de los casos para rendir finales)
                        {
                            cumpleRequisito = correlativaAprobada;
                        }

                        if (!cumpleRequisito)
                        {
                            faltantes.Add($"{req.IdPlanMateriaRequisitoNavigation.IdMateriaNavigation.Nombre} ({condicionExigida})");
                        }
                    }

                    if (faltantes.Any())
                    {
                        dto.PuedeInscribirse = false;
                        dto.MotivoBloqueo = $"Requisitos Pendientes: {string.Join(" | ", faltantes)}";
                    }
                }

                oferta.Add(dto);
            }

            return oferta.OrderBy(o => o.AnioCursada).ThenBy(o => o.MateriaNombre).ToList();
        }

        public async Task<List<MesaFinalOfertaDTO>> GetMisInscripcionesAsync(int idAlumno, int idPeriodo)
        {
            var inscripciones = await _context.InscripcionFinals
                .Include(i => i.IdMesaFinalNavigation).ThenInclude(m => m.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(i => i.IdMesaFinalNavigation).ThenInclude(m => m.IdPresidenteMesaNavigation).ThenInclude(d => d.IdUsuarioNavigation)
                .Where(i => i.IdAlumno == idAlumno && i.IdMesaFinalNavigation.IdPeriodo == idPeriodo && i.Estado != "Baja")
                .ToListAsync();

            return inscripciones.Select(i => new MesaFinalOfertaDTO
            {
                IdMesaFinal = i.IdMesaFinal,
                IdInscripcionFinal = i.Id,
                MateriaNombre = i.IdMesaFinalNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                FechaHora = i.IdMesaFinalNavigation.FechaHora,
                Tribunal = $"{i.IdMesaFinalNavigation.IdPresidenteMesaNavigation.IdUsuarioNavigation.Apellido}",
                Condicion = i.Estado ?? "Regular",
                YaInscripto = true
            }).ToList();
        }

        public async Task<ResultadoOperacionDTO> InscribirAlumnoAsync(InscripcionFinalRequestDTO dto)
        {
            var mesa = await _context.MesaFinals.Include(m => m.IdPeriodoNavigation).FirstOrDefaultAsync(m => m.Id == dto.IdMesaFinal);
            if (mesa == null) return new ResultadoOperacionDTO { Exito = false, Mensaje = "Mesa no encontrada." };

            var alumno = await _context.Alumnos.FindAsync(dto.IdAlumno);

            // Validar Ventana Operativa
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var ventana = await _context.Set<VentanaOperativa>().FirstOrDefaultAsync(v =>
                v.IdPeriodo == mesa.IdPeriodo &&
                v.TipoAccion == "INSCRIPCION_FINAL" &&
                (v.IdSede == null || v.IdSede == alumno.IdSede));

            if (ventana != null)
            {
                var inicio = DateOnly.FromDateTime(ventana.FechaInicio);
                var fin = DateOnly.FromDateTime(ventana.FechaFin);
                if (hoy < inicio || hoy > fin) return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Fuera de término (Habilitado del {inicio:dd/MM} al {fin:dd/MM})." };
            }

            var nuevaInscripcion = new InscripcionFinal
            {
                IdAlumno = dto.IdAlumno,
                IdMesaFinal = dto.IdMesaFinal,
                FechaInscripcion = DateTime.Now,
                Estado = dto.Condicion
            };

            _context.InscripcionFinals.Add(nuevaInscripcion);
            await _context.SaveChangesAsync();

            return new ResultadoOperacionDTO { Exito = true, Mensaje = $"Inscripción a final exitosa (Condición: {dto.Condicion})." };
        }

        public async Task<ResultadoOperacionDTO> CancelarInscripcionAsync(int idInscripcion, int idAlumno)
        {
            var inscripcion = await _context.InscripcionFinals
                .Include(i => i.IdMesaFinalNavigation)
                .FirstOrDefaultAsync(i => i.Id == idInscripcion && i.IdAlumno == idAlumno);

            if (inscripcion == null) return new ResultadoOperacionDTO { Exito = false, Mensaje = "No encontrada." };

            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var ventana = await _context.Set<VentanaOperativa>().FirstOrDefaultAsync(v =>
                v.IdPeriodo == inscripcion.IdMesaFinalNavigation.IdPeriodo &&
                v.TipoAccion == "INSCRIPCION_FINAL");

            if (ventana != null)
            {
                var inicio = DateOnly.FromDateTime(ventana.FechaInicio);
                var fin = DateOnly.FromDateTime(ventana.FechaFin);
                if (hoy < inicio || hoy > fin) return new ResultadoOperacionDTO { Exito = false, Mensaje = "El período de inscripciones/bajas ya finalizó." };
            }

            inscripcion.Estado = "Baja";
            await _context.SaveChangesAsync();

            return new ResultadoOperacionDTO { Exito = true, Mensaje = "Inscripción cancelada." };
        }
    }
}