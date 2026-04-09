using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class ReportesRepository : IReportesRepository
    {
        private readonly EduSysDbContext _context;

        public ReportesRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<ConstanciaInscripcionDTO> GetDatosConstanciaAsync(int idAlumno, int idPeriodo)
        {
            // 🚀 OPTIMIZADO: AsNoTracking
            var alumno = await _context.Alumnos
                .AsNoTracking()
                .Include(a => a.IdUsuarioNavigation)
                .Include(a => a.IdPlanActualNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .Include(a => a.IdSedeNavigation)
                .FirstOrDefaultAsync(a => a.Id == idAlumno);

            if (alumno == null) return null;

            var periodo = await _context.PeriodoAcademicos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == idPeriodo);

            var inscripciones = await _context.InscripcionCursada
                .AsNoTracking()
                .Include(i => i.IdComisionNavigation).ThenInclude(c => c.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(i => i.IdComisionNavigation).ThenInclude(c => c.HorarioComisions)
                .Where(i => i.IdAlumno == idAlumno && i.IdComisionNavigation.IdPeriodo == idPeriodo && i.Estado != "Baja")
                .ToListAsync();

            return new ConstanciaInscripcionDTO
            {
                InstitucionNombre = "EduSys - Instituto de Tecnología",
                FechaEmision = DateTime.Now,
                PeriodoAcademico = periodo?.Nombre ?? "Ciclo Lectivo",
                AlumnoNombre = $"{alumno.IdUsuarioNavigation.Apellido}, {alumno.IdUsuarioNavigation.Nombre}",
                Dni = alumno.IdUsuarioNavigation.Dni,
                Legajo = alumno.Legajo,
                Carrera = alumno.IdPlanActualNavigation?.IdCarreraNavigation?.Nombre ?? "-",
                Sede = alumno.IdSedeNavigation?.Nombre ?? "Central",
                Materias = inscripciones.Select(i => new DetalleMateriaConstanciaDTO
                {
                    CodigoMateria = i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Codigo ?? "-",
                    Materia = i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    Comision = i.IdComisionNavigation.Codigo,
                    AnioCursada = i.IdComisionNavigation.IdPlanMateriaNavigation.AnioCursada,
                    FechaInscripcion = i.FechaInscripcion ?? DateTime.Now,
                    Horarios = string.Join(" / ", i.IdComisionNavigation.HorarioComisions
                        .Select(h => $"{h.DiaSemana.Substring(0, 3)} {h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm}"))
                })
                .OrderBy(x => x.AnioCursada).ThenBy(x => x.Materia).ToList()
            };
        }

        public async Task<List<InscripcionGlobalDTO>> GetReporteGlobalAsync(int idPeriodo, int? idCarrera)
        {
            var query = _context.InscripcionCursada
                .AsNoTracking()
                .Where(i => i.IdComisionNavigation.IdPeriodo == idPeriodo && i.Estado != "Baja");

            if (idCarrera.HasValue && idCarrera.Value > 0)
            {
                query = query.Where(i => i.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarrera == idCarrera.Value);
            }

            // 🚀 OPTIMIZACIÓN: Proyección directa para reporte masivo
            return await query.Select(i => new InscripcionGlobalDTO
            {
                IdInscripcion = i.Id,
                Fecha = i.FechaInscripcion ?? DateTime.Now,
                AlumnoNombre = i.IdAlumnoNavigation.IdUsuarioNavigation.Apellido + ", " + i.IdAlumnoNavigation.IdUsuarioNavigation.Nombre,
                AlumnoLegajo = i.IdAlumnoNavigation.Legajo,
                AlumnoDni = i.IdAlumnoNavigation.IdUsuarioNavigation.Dni,
                Carrera = i.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarreraNavigation.Nombre,
                Materia = i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                Comision = i.IdComisionNavigation.Codigo + " (" + i.IdComisionNavigation.Turno + ")",
                Estado = i.Estado
            })
            .OrderByDescending(x => x.Fecha)
            .ToListAsync();
        }

        public async Task<CertificadoAlumnoRegularDTO> GetDatosCertificadoRegularAsync(int idAlumno, int idPeriodo)
        {
            var alumno = await _context.Alumnos
                .AsNoTracking()
                .Include(a => a.IdUsuarioNavigation)
                .Include(a => a.IdPlanActualNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .Include(a => a.IdSedeNavigation)
                .FirstOrDefaultAsync(a => a.Id == idAlumno);

            if (alumno == null) return null;

            var periodo = await _context.PeriodoAcademicos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == idPeriodo);

            bool esRegular = await _context.InscripcionCursada
                .AnyAsync(i => i.IdAlumno == idAlumno && i.IdComisionNavigation.IdPeriodo == idPeriodo && i.Estado != "Baja");

            if (!esRegular) return null;

            return new CertificadoAlumnoRegularDTO
            {
                InstitucionNombre = "EduSys - Instituto de Tecnología",
                FechaEmision = DateTime.Now,
                PeriodoAcademico = periodo?.Nombre ?? "Ciclo Lectivo",
                AlumnoNombre = $"{alumno.IdUsuarioNavigation.Apellido}, {alumno.IdUsuarioNavigation.Nombre}",
                Dni = alumno.IdUsuarioNavigation.Dni,
                Legajo = alumno.Legajo,
                Carrera = alumno.IdPlanActualNavigation?.IdCarreraNavigation?.Nombre ?? "-",
                Sede = alumno.IdSedeNavigation?.Nombre ?? "Central",
                Ciudad = "Santa Rosa",
                Provincia = "La Pampa"
            };
        }

        public async Task<List<AlumnoResumenInscripcionDTO>> GetAlumnosInscriptosAsync(int idPeriodo, int idCarrera, int? idSede)
        {
            var query = _context.InscripcionCursada
                .AsNoTracking()
                .Where(i => i.IdComisionNavigation.IdPeriodo == idPeriodo
                         && i.Estado != "Baja"
                         && i.IdComisionNavigation.IdPlanMateriaNavigation.IdPlanNavigation.IdCarrera == idCarrera);

            if (idSede.HasValue && idSede.Value > 0)
                query = query.Where(i => i.IdComisionNavigation.IdSede == idSede.Value);

            return await query
                .GroupBy(i => i.IdAlumno)
                .Select(g => new AlumnoResumenInscripcionDTO
                {
                    IdAlumno = g.Key,
                    NombreCompleto = g.First().IdAlumnoNavigation.IdUsuarioNavigation.Apellido + ", " + g.First().IdAlumnoNavigation.IdUsuarioNavigation.Nombre,
                    Legajo = g.First().IdAlumnoNavigation.Legajo,
                    Dni = g.First().IdAlumnoNavigation.IdUsuarioNavigation.Dni,
                    CantidadMaterias = g.Count()
                })
                .ToListAsync();
        }

        public async Task<HistoriaAcademicaDTO> GetHistoriaAcademicaAsync(int idAlumno)
        {
            var alumno = await _context.Alumnos
                .AsNoTracking()
                .Include(a => a.IdUsuarioNavigation)
                .Include(a => a.IdPlanActualNavigation).ThenInclude(p => p.IdCarreraNavigation)
                .FirstOrDefaultAsync(a => a.Id == idAlumno);

            if (alumno == null || alumno.IdPlanActual == null) return null;

            int idPlan = alumno.IdPlanActual.Value;

            // 🚀 PARALELISMO: Ejecutar consultas pesadas al mismo tiempo
            var planMateriasTask = _context.PlanMateria.AsNoTracking().Include(pm => pm.IdMateriaNavigation).Where(pm => pm.IdPlan == idPlan).ToListAsync();
            var cursadasTask = _context.InscripcionCursada.AsNoTracking().Include(ic => ic.IdComisionNavigation).Where(ic => ic.IdAlumno == idAlumno && ic.Estado != "Baja").ToListAsync();
            var finalesTask = _context.InscripcionFinals.AsNoTracking().Include(f => f.IdMesaFinalNavigation).Where(f => f.IdAlumno == idAlumno && f.Estado != "Anulado").ToListAsync();

            await Task.WhenAll(planMateriasTask, cursadasTask, finalesTask);

            var planMaterias = planMateriasTask.Result;
            var cursadas = cursadasTask.Result;
            var finales = finalesTask.Result;

            var historia = new HistoriaAcademicaDTO
            {
                AlumnoNombre = $"{alumno.IdUsuarioNavigation.Apellido}, {alumno.IdUsuarioNavigation.Nombre}",
                Legajo = alumno.Legajo,
                Carrera = alumno.IdPlanActualNavigation.IdCarreraNavigation.Nombre,
                Plan = alumno.IdPlanActualNavigation.Nombre,
                TotalMateriasPlan = planMaterias.Count,
                Detalle = new List<DetalleMateriaAvanceDTO>()
            };

            decimal sumaNotas = 0;
            int cantidadAprobadas = 0;

            foreach (var pm in planMaterias)
            {
                var item = new DetalleMateriaAvanceDTO
                {
                    IdPlanMateria = pm.Id,
                    AnioCursada = pm.AnioCursada,
                    Materia = pm.IdMateriaNavigation.Nombre,
                    Codigo = pm.IdMateriaNavigation.Codigo ?? "-",
                    Estado = "Pendiente"
                };

                var finalAprobado = finales.Where(f => f.IdMesaFinalNavigation.IdPlanMateria == pm.Id && f.Nota >= (pm.NotaMinimaAprobacion ?? 4)).OrderByDescending(f => f.FechaInscripcion).FirstOrDefault();

                if (finalAprobado != null)
                {
                    item.Estado = "Aprobada";
                    item.Nota = finalAprobado.Nota;
                    item.Fecha = finalAprobado.FechaInscripcion;
                    if (item.Nota.HasValue) { sumaNotas += item.Nota.Value; cantidadAprobadas++; }
                }
                else
                {
                    var cursada = cursadas.FirstOrDefault(c => c.IdComisionNavigation.IdPlanMateria == pm.Id);
                    if (cursada != null)
                    {
                        item.Estado = cursada.CondicionFinal ?? cursada.Estado;
                        if (cursada.CondicionFinal == "Promocionado")
                        {
                            item.Nota = cursada.NotaFinalCursada;
                            item.Fecha = cursada.FechaInscripcion;
                            if (item.Nota.HasValue) { sumaNotas += item.Nota.Value; cantidadAprobadas++; }
                        }
                    }
                }
                historia.Detalle.Add(item);
            }

            historia.MateriasAprobadas = cantidadAprobadas;
            if (cantidadAprobadas > 0) historia.PromedioGeneral = Math.Round(sumaNotas / cantidadAprobadas, 2);
            if (historia.TotalMateriasPlan > 0) historia.PorcentajeAvance = Math.Round(((double)cantidadAprobadas / historia.TotalMateriasPlan) * 100, 2);

            return historia;
        }

        public async Task<ConstanciaFinalDTO?> GetDatosConstanciaFinalAsync(int idInscripcion, int idAlumno)
        {
            return await _context.InscripcionFinals
                .AsNoTracking()
                .Where(i => i.Id == idInscripcion && i.IdAlumno == idAlumno && i.Estado != "Baja")
                .Select(i => new ConstanciaFinalDTO
                {
                    AlumnoNombreCompleto = i.IdAlumnoNavigation.IdUsuarioNavigation.Apellido + ", " + i.IdAlumnoNavigation.IdUsuarioNavigation.Nombre,
                    AlumnoDNI = i.IdAlumnoNavigation.IdUsuarioNavigation.Dni,
                    AlumnoLegajo = i.IdAlumnoNavigation.Legajo,
                    CarreraNombre = i.IdAlumnoNavigation.IdPlanActualNavigation != null ? i.IdAlumnoNavigation.IdPlanActualNavigation.IdCarreraNavigation.Nombre : "Sin Carrera",
                    MateriaNombre = i.IdMesaFinalNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    FechaExamen = i.IdMesaFinalNavigation.FechaHora,
                    Tribunal = i.IdMesaFinalNavigation.IdPresidenteMesaNavigation.IdUsuarioNavigation.Apellido + " (Pres.)",
                    Condicion = i.Estado ?? "Regular",
                    FechaInscripcion = i.FechaInscripcion ?? DateTime.Now,
                    NumeroTransaccion = i.Id
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ActaIndividualDTO?> GetDatosActaIndividualAsync(int idActa)
        {
            // Cargamos el acta con TODAS las rutas posibles hacia el nombre del docente
            var acta = await _context.ActaAlumnos
                .AsNoTracking()
                .Include(a => a.IdAlumnoNavigation).ThenInclude(al => al.IdUsuarioNavigation)
                .Include(a => a.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdMateriaNavigation)
                .Include(a => a.IdPlanMateriaNavigation).ThenInclude(pm => pm.IdPlanNavigation).ThenInclude(p => p.IdCarreraNavigation)
                // Ruta 1: Docente que cerró el acta directamente
                .Include(a => a.IdDocenteFirmaNavigation).ThenInclude(d => d.IdUsuarioNavigation)
                // Ruta 2: Docentes de la comisión (si es parcial/recu)
                .Include(a => a.IdEvaluacionReferenciaNavigation)
                    .ThenInclude(e => e.IdComisionNavigation)
                        .ThenInclude(c => c.DocenteComisions)
                            .ThenInclude(dc => dc.IdDocenteNavigation)
                                .ThenInclude(d => d.IdUsuarioNavigation)
                // Ruta 3: Docentes de la comisión (si es cierre de cursada)
                .Include(a => a.IdInscripcionCursadaReferenciaNavigation)
                    .ThenInclude(i => i.IdComisionNavigation)
                        .ThenInclude(c => c.DocenteComisions)
                            .ThenInclude(dc => dc.IdDocenteNavigation)
                                .ThenInclude(d => d.IdUsuarioNavigation)
                .FirstOrDefaultAsync(a => a.Id == idActa);

            if (acta == null) return null;

            // Lógica para determinar qué nombre mostrar en la firma
            string nombreProfesor = "Secretaría Académica";

            // Intentamos obtener el docente titular o el primero asignado
            var docentesComision = acta.IdEvaluacionReferenciaNavigation?.IdComisionNavigation?.DocenteComisions
                                ?? acta.IdInscripcionCursadaReferenciaNavigation?.IdComisionNavigation?.DocenteComisions;

            if (acta.IdDocenteFirmaNavigation != null)
            {
                nombreProfesor = $"{acta.IdDocenteFirmaNavigation.IdUsuarioNavigation.Nombre} {acta.IdDocenteFirmaNavigation.IdUsuarioNavigation.Apellido}";
            }
            else if (docentesComision != null && docentesComision.Any())
            {
                var docentePrincipal = docentesComision.FirstOrDefault(dc => dc.RolDocente == "Titular")
                                     ?? docentesComision.First();

                nombreProfesor = $"{docentePrincipal.IdDocenteNavigation.IdUsuarioNavigation.Nombre} {docentePrincipal.IdDocenteNavigation.IdUsuarioNavigation.Apellido}";
            }

            return new ActaIndividualDTO
            {
                IdActa = acta.Id,
                NumeroActa = acta.NumeroActa,
                FechaEmision = acta.FechaEmision,
                TipoActa = acta.TipoActa,
                Detalle = acta.Detalle,
                Nota = acta.Nota,
                EstadoAcademico = acta.EstadoAcademico,
                AlumnoNombre = $"{acta.IdAlumnoNavigation.IdUsuarioNavigation.Nombre} {acta.IdAlumnoNavigation.IdUsuarioNavigation.Apellido}",
                DNI = acta.IdAlumnoNavigation.IdUsuarioNavigation.Dni,
                Legajo = acta.IdAlumnoNavigation.Legajo,
                MateriaNombre = acta.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre ?? "S/D",
                CarreraNombre = acta.IdPlanMateriaNavigation.IdPlanNavigation.IdCarreraNavigation.Nombre ?? "S/D",
                Sede = acta.IdAlumnoNavigation.IdSedeNavigation?.Nombre ?? "Sede Central",
                DocenteFirma = nombreProfesor // <--- Aquí pasamos el nombre detectado
            };
        }
    }
}