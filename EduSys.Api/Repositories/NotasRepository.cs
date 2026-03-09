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
                .Include(c => c.InscripcionCursada).ThenInclude(i => i.IdEstadoMateriaNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == idComision);

            if (comision == null) return null;

            var planilla = new PlanillaNotasDTO
            {
                IdComision = idComision,
                MateriaNombre = comision.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                ComisionCodigo = comision.Codigo,
                ComisionEstado = comision.Estado ?? "Abierta",
                Evaluaciones = comision.Evaluacions.Select(e => new EvaluacionDTO
                {
                    IdEvaluacion = e.Id,
                    Nombre = e.Nombre,
                    Fecha = e.Fecha.ToDateTime(TimeOnly.MinValue),
                    EsRecuperatorio = e.EsRecuperatorio == true,
                    EsIntegrador = e.EsIntegrador == true,
                    IdEvaluacionPadre = e.IdEvaluacionPadre,
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
                    Estado = ins.IdEstadoMateriaNavigation?.Nombre ?? "Cursando",
                    CursadaCerrada = ins.CursadaCerrada
                };

                foreach (var eval in planilla.Evaluaciones)
                {
                    var notaExistente = ins.Nota.FirstOrDefault(n => n.IdEvaluacion == eval.IdEvaluacion);
                    fila.Notas[eval.IdEvaluacion] = notaExistente?.Valor;
                }

                var notasValidas = fila.Notas.Values.Where(v => v.HasValue).Select(v => v!.Value);
                if (notasValidas.Any())
                    fila.Promedio = Math.Round(notasValidas.Average(), 2);

                planilla.Alumnos.Add(fila);
            }

            return planilla;
        }

        public async Task<bool> GuardarNotaAsync(int idInscripcion, int idEvaluacion, decimal? valor)
        {
            try
            {
                var evaluacion = await _context.Evaluacions.AsNoTracking().FirstOrDefaultAsync(e => e.Id == idEvaluacion);
                if (evaluacion == null || evaluacion.EstadoActa == "Cerrada") return false;

                var nota = await _context.Nota
                    .FirstOrDefaultAsync(n => n.IdInscripcionCursada == idInscripcion && n.IdEvaluacion == idEvaluacion);

                if (valor.HasValue)
                {
                    if (nota == null)
                    {
                        nota = new Nota
                        {
                            IdInscripcionCursada = idInscripcion,
                            IdEvaluacion = idEvaluacion,
                            Valor = valor.Value,
                            FechaCarga = DateTime.Now
                        };
                        await _context.Nota.AddAsync(nota);
                    }
                    else
                    {
                        nota.Valor = valor.Value;
                        nota.FechaCarga = DateTime.Now;
                    }
                }
                else
                {
                    if (nota != null) _context.Nota.Remove(nota);
                }

                await _context.SaveChangesAsync();

                // ✅ CORRECCIÓN: Separamos el recalculo del guardado principal para evitar choques en memoria
                await RecalcularEstadoAlumnoAsync(idInscripcion);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error GuardarNota: {ex.Message} - {ex.InnerException?.Message}");
                return false;
            }
        }

        // =======================================================================
        // 🧠 MOTOR DE REGLAS ACADÉMICAS
        // =======================================================================
        private (int IdEstado, decimal? NotaFinal) CalcularEstadoYNotaDefinitiva(PlanMateria plan, List<Nota> notasAlumno, List<Evaluacion> evaluacionesComision, int idEstadoActual)
        {
            if (!notasAlumno.Any()) return (idEstadoActual, null);

            int estadoDesaprobado = plan.IdEstadoSiDesaprueba ?? 7;
            int estadoLibre = plan.IdEstadoSiFaltaAsistencia ?? 4;
            int estadoRegular = plan.IdEstadoRegular ?? 2;

            // 1. MUERTE SÚBITA
            if (plan.NotaEliminatoria.HasValue && notasAlumno.Any(n => n.Valor < plan.NotaEliminatoria.Value))
            {
                var notasValidas = notasAlumno.Select(n => n.Valor).ToList();
                decimal prom = notasValidas.Any() ? Math.Round(notasValidas.Average(), 2) : 0m;
                return (estadoLibre, prom);
            }

            // 2. SEPARAR EXÁMENES
            var integrador = evaluacionesComision.FirstOrDefault(e => e.EsIntegrador == true);
            var parcialesPrincipales = evaluacionesComision.Where(e => e.IdEvaluacionPadre == null && e.EsIntegrador != true).ToList();

            var notasPorInstancia = new List<decimal>();

            // 3. PISAR NOTAS O MAX
            foreach (var parcial in parcialesPrincipales)
            {
                decimal? notaParcial = notasAlumno.FirstOrDefault(n => n.IdEvaluacion == parcial.Id)?.Valor;
                var recuperatorios = evaluacionesComision.Where(e => e.IdEvaluacionPadre == parcial.Id).ToList();

                bool rindioInstancia = notaParcial.HasValue || notasAlumno.Any(n => recuperatorios.Select(r => r.Id).Contains(n.IdEvaluacion));

                if (rindioInstancia)
                {
                    decimal notaDefinitiva = notaParcial ?? 0m;

                    var notasRecusRendidos = notasAlumno.Where(n => recuperatorios.Select(r => r.Id).Contains(n.IdEvaluacion)).ToList();
                    if (notasRecusRendidos.Any())
                    {
                        if (plan.ModoNotaRecuperatorio == 0) // Max
                        {
                            decimal maxRecu = notasRecusRendidos.Max(n => n.Valor);
                            notaDefinitiva = Math.Max(notaDefinitiva, maxRecu);
                        }
                        else // Pisar
                        {
                            var ultimoRecuRendido = notasRecusRendidos.OrderByDescending(n => n.FechaCarga).First();
                            notaDefinitiva = ultimoRecuRendido.Valor;
                        }
                    }
                    notasPorInstancia.Add(notaDefinitiva);
                }
            }

            if (!notasPorInstancia.Any()) return (idEstadoActual, null);

            decimal promedioInstancias = Math.Round(notasPorInstancia.Average(), 2);
            decimal notaDeCorte = plan.PromedioMinimoAprobacion ?? plan.NotaMinimaRegularizar ?? 4m;
            int cantidadAplazos = notasPorInstancia.Count(n => n < notaDeCorte);

            // 4. LÍMITES Y ASISTENCIA
            decimal porcentajeAsistencia = 100m;
            if (plan.PorcentajeAsistenciaRegularizar.HasValue && porcentajeAsistencia < plan.PorcentajeAsistenciaRegularizar.Value)
                return (estadoLibre, promedioInstancias);

            if (plan.CantidadAplazosParaLibre.HasValue && cantidadAplazos > plan.CantidadAplazosParaLibre.Value)
                return (estadoLibre, promedioInstancias);

            // 5. RENDIMIENTO FINAL
            if (plan.CantidadParciales.HasValue && notasPorInstancia.Count >= plan.CantidadParciales.Value)
            {
                bool aproboCursadaBase = plan.ModoAprobacionCursada == 1
                    ? notasPorInstancia.All(n => n >= notaDeCorte)
                    : promedioInstancias >= notaDeCorte;

                if (aproboCursadaBase)
                {
                    bool cumplePromo = plan.NotaMinimaPromocion.HasValue &&
                        (plan.ModoAprobacionCursada == 1 ? notasPorInstancia.All(n => n >= plan.NotaMinimaPromocion.Value) : promedioInstancias >= plan.NotaMinimaPromocion.Value);

                    if (plan.EsPromocionable == true && cumplePromo && plan.IdEstadoPromocion.HasValue)
                        return (plan.IdEstadoPromocion.Value, promedioInstancias);

                    return (estadoRegular, null);
                }
                else
                {
                    // 6. INTEGRADOR
                    int parcialesAprobados = notasPorInstancia.Count(n => n >= notaDeCorte);
                    bool tieneDerechoIntegrador = plan.TieneIntegrador &&
                        (!plan.CondicionIntegradorParciales.HasValue || parcialesAprobados >= plan.CondicionIntegradorParciales.Value);

                    if (tieneDerechoIntegrador && integrador != null)
                    {
                        decimal? notaIntegrador = notasAlumno.FirstOrDefault(n => n.IdEvaluacion == integrador.Id)?.Valor;

                        if (notaIntegrador.HasValue)
                        {
                            decimal minAprobarInt = plan.NotaAprobacionIntegrador ?? notaDeCorte;

                            if (notaIntegrador.Value >= minAprobarInt)
                            {
                                if (plan.IntegradorPermitePromocion && plan.NotaPromocionIntegrador.HasValue &&
                                    notaIntegrador.Value >= plan.NotaPromocionIntegrador.Value && plan.IdEstadoPromocion.HasValue)
                                {
                                    return (plan.IdEstadoPromocion.Value, notaIntegrador.Value);
                                }

                                return (estadoRegular, null);
                            }
                            else
                            {
                                return (estadoDesaprobado, notaIntegrador.Value);
                            }
                        }
                        else
                        {
                            return (idEstadoActual, promedioInstancias);
                        }
                    }

                    return (estadoDesaprobado, promedioInstancias);
                }
            }

            return (idEstadoActual, promedioInstancias);
        }

        private async Task RecalcularEstadoAlumnoAsync(int idInscripcionCursada)
        {
            try
            {
                // Buscamos la inscripción en una nueva consulta limpia
                var inscripcion = await _context.InscripcionCursada
                    .Include(i => i.Nota)
                    .Include(i => i.IdComisionNavigation).ThenInclude(c => c.IdPlanMateriaNavigation)
                    .FirstOrDefaultAsync(i => i.Id == idInscripcionCursada);

                if (inscripcion == null || inscripcion.CursadaCerrada) return;

                var evaluacionesComision = await _context.Evaluacions
                    .Where(e => e.IdComision == inscripcion.IdComision)
                    .AsNoTracking()
                    .ToListAsync();

                var resultado = CalcularEstadoYNotaDefinitiva(
                    inscripcion.IdComisionNavigation.IdPlanMateriaNavigation,
                    inscripcion.Nota.ToList(),
                    evaluacionesComision,
                    inscripcion.IdEstadoMateria ?? 1);

                inscripcion.IdEstadoMateria = resultado.IdEstado;
                inscripcion.NotaFinalCursada = resultado.NotaFinal;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al recalcular: {ex.Message}");
            }
        }

        public async Task<bool> CerrarActaComisionAsync(int idComision, string libro, string folio)
        {
            var comision = await _context.Comisions
                .Include(c => c.IdPlanMateriaNavigation)
                .FirstOrDefaultAsync(c => c.Id == idComision);

            if (comision == null) return false;

            var evaluacionesComision = await _context.Evaluacions.Where(e => e.IdComision == idComision).AsNoTracking().ToListAsync();

            var inscripciones = await _context.InscripcionCursada
                .Include(i => i.Nota)
                .Where(i => i.IdComision == idComision && i.Estado != "Baja")
                .ToListAsync();

            foreach (var inscripcion in inscripciones)
            {
                if (inscripcion.CursadaCerrada) continue;

                var resultado = CalcularEstadoYNotaDefinitiva(comision.IdPlanMateriaNavigation, inscripcion.Nota.ToList(), evaluacionesComision, inscripcion.IdEstadoMateria ?? 1);

                inscripcion.IdEstadoMateria = resultado.IdEstado;
                inscripcion.NotaFinalCursada = resultado.NotaFinal;
                inscripcion.Estado = "Finalizada";
                inscripcion.CursadaCerrada = true;
            }

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

            if (result) await _notificationService.NotificarCierreActaAsync(evaluacion.Id, evaluacion.Nombre);
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
            eval.EsIntegrador = dto.EsIntegrador;
            eval.IdEvaluacionPadre = dto.IdEvaluacionPadre;

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
                EsIntegrador = dto.EsIntegrador,
                IdEvaluacionPadre = dto.IdEvaluacionPadre,
                EstadoActa = "Abierta"
            };
            _context.Evaluacions.Add(nuevaEval);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ReabrirActaAsync(int idEvaluacion)
        {
            var eval = await _context.Evaluacions.FindAsync(idEvaluacion);
            if (eval == null || eval.EstadoActa != "Cerrada") return false;

            eval.EstadoActa = "Abierta";
            eval.FechaCierre = null;
            eval.Libro = null;
            eval.Folio = null;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarEvaluacionAsync(int idEvaluacion)
        {
            try
            {
                // ✅ CORRECCIÓN 400 AL ELIMINAR: 
                // Primero buscamos el examen directamente sin Incluir notas
                var evaluacion = await _context.Evaluacions.FirstOrDefaultAsync(e => e.Id == idEvaluacion);
                if (evaluacion == null || evaluacion.EstadoActa == "Cerrada") return false;

                // Desvinculamos a todos los hijos (recuperatorios) de este examen
                var hijos = await _context.Evaluacions.Where(e => e.IdEvaluacionPadre == idEvaluacion).ToListAsync();
                foreach (var hijo in hijos)
                {
                    hijo.IdEvaluacionPadre = null;
                    hijo.EsRecuperatorio = false; // Como perdió al padre, ya no es recuperatorio
                }

                // Guardamos la desvinculación para liberar las llaves foráneas
                await _context.SaveChangesAsync();

                // Buscamos TODAS las notas atadas a este examen y las borramos
                var notasABorrar = await _context.Nota.Where(n => n.IdEvaluacion == idEvaluacion).ToListAsync();
                if (notasABorrar.Any())
                {
                    _context.Nota.RemoveRange(notasABorrar);
                    await _context.SaveChangesAsync(); // Guardamos el borrado de notas
                }

                // Finalmente borramos el examen
                _context.Evaluacions.Remove(evaluacion);
                await _context.SaveChangesAsync();

                // Recalculamos los estados de toda la comisión porque borramos una columna de notas
                var inscripciones = await _context.InscripcionCursada
                    .Where(i => i.IdComision == evaluacion.IdComision && i.Estado != "Baja")
                    .Select(i => i.Id)
                    .ToListAsync();

                foreach (var insId in inscripciones)
                {
                    await RecalcularEstadoAlumnoAsync(insId);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar: {ex.Message} - {ex.InnerException?.Message}");
                return false;
            }
        }

        public async Task<bool> ToggleCierreCursadaIndividualAsync(int idInscripcion)
        {
            var inscripcion = await _context.InscripcionCursada.FindAsync(idInscripcion);
            if (inscripcion == null) return false;

            inscripcion.CursadaCerrada = !inscripcion.CursadaCerrada;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReabrirActaComisionAsync(int idComision)
        {
            var comision = await _context.Comisions.Include(c => c.InscripcionCursada).FirstOrDefaultAsync(c => c.Id == idComision);
            if (comision == null) return false;

            comision.Estado = "Abierta";
            foreach (var ins in comision.InscripcionCursada) ins.CursadaCerrada = false;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}