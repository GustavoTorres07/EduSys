using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class NotasRepository : INotasRepository
    {
        private readonly EduSysDbContext _context;
        private readonly INotificationService _notificationService;

        public NotasRepository(EduSysDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<PlanillaNotasDTO> GetPlanillaAsync(int idComision)
        {
            var comision = await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation.IdMateriaNavigation)
                .Include(c => c.Evaluacions)
                .Include(c => c.InscripcionCursada).ThenInclude(i => i.IdAlumnoNavigation.IdUsuarioNavigation)
                .Include(c => c.InscripcionCursada).ThenInclude(i => i.Nota)
                .FirstOrDefaultAsync(c => c.Id == idComision);

            if (comision == null) return null;

            var planilla = new PlanillaNotasDTO
            {
                IdComision = idComision,
                MateriaNombre = comision.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                ComisionCodigo = comision.Codigo,
                Evaluaciones = comision.Evaluacions.Select(e => new EvaluacionDTO
                {
                    IdEvaluacion = e.Id,
                    Nombre = e.Nombre,
                    Fecha = e.Fecha.ToDateTime(TimeOnly.MinValue),
                    EsRecuperatorio = e.EsRecuperatorio ?? false,
                    EstadoActa = e.EstadoActa ?? "Abierta",
                    Libro = e.Libro,
                    Folio = e.Folio,
                    FechaCierre = e.FechaCierre
                }).OrderBy(e => e.Fecha).ToList()
            };

            foreach (var ins in comision.InscripcionCursada.Where(i => i.Estado != "Baja"))
            {
                var fila = new NotaAlumnoDTO
                {
                    IdInscripcion = ins.Id,
                    AlumnoNombre = $"{ins.IdAlumnoNavigation.IdUsuarioNavigation.Apellido}, {ins.IdAlumnoNavigation.IdUsuarioNavigation.Nombre}",
                    Legajo = ins.IdAlumnoNavigation.Legajo,
                    Estado = ins.CondicionFinal ?? "Cursando"
                };

                foreach (var eval in planilla.Evaluaciones)
                {
                    var notaExistente = ins.Nota.FirstOrDefault(n => n.IdEvaluacion == eval.IdEvaluacion);
                    // Aquí asignamos el valor (que puede ser null si no hay nota)
                    fila.Notas[eval.IdEvaluacion] = notaExistente?.Valor;
                }

                var notasValidas = fila.Notas.Values.Where(v => v.HasValue).Select(v => v!.Value);
                if (notasValidas.Any())
                    fila.Promedio = Math.Round(notasValidas.Average(), 2);

                planilla.Alumnos.Add(fila);
            }

            return planilla;
        }

        // ✅ MÉTODO ACTUALIZADO PARA MANEJAR NULOS (BORRADO)
        public async Task<bool> GuardarNotaAsync(int idInscripcion, int idEvaluacion, decimal? valor)
        {
            // 1. Verificación de bloqueo (Acta Cerrada)
            var evaluacion = await _context.Evaluacions.FindAsync(idEvaluacion);
            if (evaluacion == null) return false;
            if (evaluacion.EstadoActa == "Cerrada") return false;

            // 2. Buscar nota existente
            var nota = await _context.Nota
                .FirstOrDefaultAsync(n => n.IdInscripcionCursada == idInscripcion && n.IdEvaluacion == idEvaluacion);

            // CASO A: Hay un valor (Insertar o Actualizar)
            if (valor.HasValue)
            {
                if (nota == null)
                {
                    // Crear nueva nota
                    nota = new Nota
                    {
                        IdInscripcionCursada = idInscripcion,
                        IdEvaluacion = idEvaluacion,
                        Valor = valor.Value, // Guardamos el valor
                        FechaCarga = DateTime.Now
                    };
                    _context.Nota.Add(nota);
                }
                else
                {
                    // Actualizar nota existente
                    nota.Valor = valor.Value;
                    nota.FechaCarga = DateTime.Now;
                    _context.Nota.Update(nota); // Marcamos explícitamente como modificado
                }
            }
            // CASO B: El valor es nulo (Borrar la nota)
            else
            {
                if (nota != null)
                {
                    _context.Nota.Remove(nota); // Eliminamos registro físico para que quede vacío (-)
                }
                else
                {
                    return true; // Ya estaba vacía, no hay nada que hacer
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        // =======================================================================
        // LÓGICA AUTOMÁTICA DE CIERRE DE CURSADA Y CÁLCULO DE CONDICIÓN FINAL
        // =======================================================================
        public async Task<bool> CerrarActaComisionAsync(int idComision, string libro, string folio)
        {
            // 1. Traemos la comisión con sus reglas (PlanMateria)
            var comision = await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation)
                .FirstOrDefaultAsync(c => c.Id == idComision);

            if (comision == null) return false;

            var reglas = comision.IdPlanMateriaNavigation;

            // 2. Traemos a todos los alumnos inscriptos con sus Notas
            var inscripciones = await _context.InscripcionCursada
                .Include(i => i.Nota)
                .Where(i => i.IdComision == idComision && i.Estado != "Baja")
                .ToListAsync();

            foreach (var inscripcion in inscripciones)
            {
                // 3. Calculamos el promedio de las notas cargadas
                decimal promedio = 0;

                // 👇 ESTA ES LA LÍNEA CORREGIDA 👇
                if (inscripcion.Nota != null && inscripcion.Nota.Any())
                {
                    // Como 'Valor' es decimal (nunca es nulo), calculamos el promedio directamente
                    promedio = inscripcion.Nota.Average(n => n.Valor);
                }

                inscripcion.NotaFinalCursada = Math.Round(promedio, 2);

                // 4. Evaluar Reglas (Asumimos Asistencia al 100% si no hay módulo de faltas)
                decimal asistenciaDelAlumno = 100;

                // Verificamos si cumple para REGULAR
                bool cumpleAsistenciaRegular = asistenciaDelAlumno >= (reglas.PorcentajeAsistenciaRegularizar ?? 0);
                bool cumpleNotaRegular = promedio >= (reglas.NotaMinimaRegularizar ?? 4);
                bool quedaRegular = cumpleNotaRegular && cumpleAsistenciaRegular;

                // Verificamos si cumple para PROMOCIÓN
                bool promociona = false;
                if (reglas.EsPromocionable == true && reglas.TieneFinalObligatorio == false)
                {
                    bool cumpleAsistenciaPromo = asistenciaDelAlumno >= (reglas.PorcentajeAsistenciaPromocion ?? 0);
                    bool cumpleNotaPromo = promedio >= (reglas.NotaMinimaPromocion ?? 7);

                    if (cumpleNotaPromo && cumpleAsistenciaPromo)
                    {
                        promociona = true;
                    }
                }

                // 5. ASIGNAR EL VEREDICTO FINAL AUTOMÁTICAMENTE
                if (promociona)
                {
                    inscripcion.CondicionFinal = "Promocionado";
                    inscripcion.Estado = "Finalizada";
                }
                else if (quedaRegular)
                {
                    inscripcion.CondicionFinal = "Regular";
                    inscripcion.Estado = "Finalizada";
                }
                else
                {
                    inscripcion.CondicionFinal = "Libre";
                    inscripcion.Estado = "Finalizada";
                }
            }

            // 6. Cambiamos el estado de la comisión
            comision.Estado = "Cerrada";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CerrarActaAsync(CierreActaDTO dto)
        {
            var evaluacion = await _context.Evaluacions.FindAsync(dto.IdEvaluacion);
            if (evaluacion == null) return false;

            evaluacion.EstadoActa = "Cerrada";
            evaluacion.FechaCierre = DateTime.Now;
            evaluacion.Libro = dto.Libro;
            evaluacion.Folio = dto.Folio;

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await _notificationService.NotificarCierreActaAsync(evaluacion.Id, evaluacion.Nombre);
            }
            return result;
        }

        public async Task<bool> EditarEvaluacionAsync(EvaluacionDTO dto)
        {
            var eval = await _context.Evaluacions.FindAsync(dto.IdEvaluacion);
            if (eval == null) return false;
            if (eval.EstadoActa == "Cerrada") return false;

            eval.Nombre = dto.Nombre;
            eval.Fecha = DateOnly.FromDateTime(dto.Fecha);
            eval.EsRecuperatorio = dto.EsRecuperatorio;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CrearEvaluacionAsync(int idComision, EvaluacionDTO dto)
        {
            var nuevaEval = new Evaluacion
            {
                IdComision = idComision,
                Nombre = dto.Nombre,
                Fecha = DateOnly.FromDateTime(dto.Fecha),
                EsRecuperatorio = dto.EsRecuperatorio,
                EstadoActa = "Abierta"
            };
            _context.Evaluacions.Add(nuevaEval);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ReabrirActaAsync(int idEvaluacion)
        {
            var eval = await _context.Evaluacions.FindAsync(idEvaluacion);
            if (eval == null) return false;

            // Solo se puede reabrir si está cerrada
            if (eval.EstadoActa != "Cerrada") return false;

            // Restauramos estado
            eval.EstadoActa = "Abierta";
            eval.FechaCierre = null;
            eval.Libro = null;
            eval.Folio = null;

            // (Opcional) Podrías guardar un log de auditoría aquí de quién la reabrió

            return await _context.SaveChangesAsync() > 0;
        }
    }
}