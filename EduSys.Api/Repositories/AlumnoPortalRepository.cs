using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Repositories
{
    public class AlumnoPortalRepository : IAlumnoPortalRepository
    {
        private readonly EduSysDbContext _context;

        public AlumnoPortalRepository(EduSysDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificacionDTO>> GetNotificacionesAsync(int idUsuario)
        {
            return await _context.Notificacions
                .AsNoTracking()
                .Where(n => n.IdUsuario == idUsuario)
                .OrderByDescending(n => n.Fecha)
                .Take(50)
                .Select(n => new NotificacionDTO
                {
                    Id = n.Id,
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    Fecha = n.Fecha,
                    Leida = n.Leida,
                    Tipo = n.Tipo ?? "Sistema"
                })
                .ToListAsync();
        }

        public async Task<bool> MarcarNotificacionLeidaAsync(int idNotificacion)
        {
            var notif = await _context.Notificacions.FindAsync(idNotificacion);
            if (notif == null) return false;

            notif.Leida = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<CursadaAlumnoDTO>> GetMisCursadasAsync(int idUsuario)
        {
            var datosDb = await _context.InscripcionCursada
                .AsNoTracking()
                .Where(i => i.IdAlumnoNavigation.IdUsuario == idUsuario && i.Estado != "Baja")
                .Select(ins => new
                {
                    IdInscripcion = ins.Id,
                    Materia = ins.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    Comision = ins.IdComisionNavigation.Codigo,
                    EstadoCursada = ins.CondicionFinal ?? ins.Estado,
                    ModoNotaRecuperatorio = ins.IdComisionNavigation.IdPlanMateriaNavigation.ModoNotaRecuperatorio,
                    ExamenesDB = ins.IdComisionNavigation.Evaluacions
                        .Select(eval => new
                        {
                            Id = eval.Id,
                            IdEvaluacionPadre = eval.IdEvaluacionPadre,
                            EsRecuperatorio = eval.EsRecuperatorio ?? false,
                            Nombre = eval.Nombre,
                            Fecha = eval.Fecha,
                            EstadoActa = eval.EstadoActa,
                            Nota = ins.Nota.Where(n => n.IdEvaluacion == eval.Id).Select(n => (decimal?)n.Valor).FirstOrDefault()
                        }).ToList()
                })
                .ToListAsync();

            var resultado = new List<CursadaAlumnoDTO>();

            foreach (var item in datosDb)
            {
                var dto = new CursadaAlumnoDTO
                {
                    IdInscripcion = item.IdInscripcion,
                    Materia = item.Materia ?? "S/N",
                    Comision = item.Comision ?? "S/C",
                    EstadoCursada = item.EstadoCursada,
                    Examenes = new List<ExamenAlumnoDTO>()
                };

                var notasEfectivasParaPromedio = new List<decimal>();
                var regulares = item.ExamenesDB.Where(e => !e.EsRecuperatorio).ToList();
                var recuperatorios = item.ExamenesDB.Where(e => e.EsRecuperatorio).ToList();

                foreach (var reg in regulares)
                {
                    decimal? notaEfectiva = reg.Nota;
                    var recup = recuperatorios.FirstOrDefault(r => r.IdEvaluacionPadre == reg.Id);

                    if (recup != null && recup.Nota.HasValue)
                    {
                        if (item.ModoNotaRecuperatorio == 1)
                            notaEfectiva = recup.Nota.Value;
                        else
                        {
                            if (reg.Nota.HasValue)
                                notaEfectiva = Math.Max(reg.Nota.Value, recup.Nota.Value);
                            else
                                notaEfectiva = recup.Nota.Value;
                        }
                    }

                    if (notaEfectiva.HasValue)
                        notasEfectivasParaPromedio.Add(notaEfectiva.Value);
                }

                foreach (var eval in item.ExamenesDB.OrderBy(e => e.Fecha))
                {
                    dto.Examenes.Add(new ExamenAlumnoDTO
                    {
                        Nombre = eval.Nombre,
                        Fecha = eval.Fecha.ToDateTime(TimeOnly.MinValue),
                        Nota = eval.Nota,
                        EsOficial = eval.EstadoActa == "Cerrada"
                    });
                }

                if (notasEfectivasParaPromedio.Any())
                    dto.Promedio = Math.Round(notasEfectivasParaPromedio.Average(), 2);

                resultado.Add(dto);
            }

            return resultado;
        }

        // 🚀 NUEVO: IMPLEMENTACIÓN DE ASISTENCIAS REALES
        public async Task<List<AsistenciaMateriaDTO>> GetMisAsistenciasAsync(int idUsuario)
        {
            var cursadasDb = await _context.InscripcionCursada
                .AsNoTracking()
                .Where(i => i.IdAlumnoNavigation.IdUsuario == idUsuario && i.Estado != "Baja")
                .Select(i => new
                {
                    MateriaNombre = i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                    ComisionCodigo = i.IdComisionNavigation.Codigo,
                    CicloLectivo = i.IdComisionNavigation.IdPeriodoNavigation.FechaInicio.Year,
                    PorcentajeRequerido = i.IdComisionNavigation.IdPlanMateriaNavigation.PorcentajeAsistenciaRegularizar ?? 0,
                    AsistenciasDb = i.Asistencia.Select(a => new
                    {
                        Fecha = a.Fecha, // Es DateOnly en BD
                        EstaPresente = a.EstaPresente,
                        EsJustificado = a.EsJustificado,
                        Observacion = a.Observacion
                    }).ToList()
                })
                .ToListAsync();

            var resultado = new List<AsistenciaMateriaDTO>();

            foreach (var cursada in cursadasDb)
            {
                var materiaDto = new AsistenciaMateriaDTO
                {
                    Materia = cursada.MateriaNombre ?? "Sin Nombre",
                    Comision = cursada.ComisionCodigo ?? "S/C",
                    CicloLectivo = cursada.CicloLectivo,
                    PorcentajeRequerido = (decimal)cursada.PorcentajeRequerido,
                    Registros = cursada.AsistenciasDb.Select(asist => new AsistenciaRegistroDTO
                    {
                        Fecha = asist.Fecha.ToDateTime(TimeOnly.MinValue), // Conversión de DateOnly a DateTime
                        Estado = asist.EsJustificado ? "Justificado" : (asist.EstaPresente ? "Presente" : "Ausente"),
                        Observacion = asist.Observacion
                    }).ToList()
                };

                resultado.Add(materiaDto);
            }

            return resultado.OrderByDescending(a => a.CicloLectivo).ThenBy(a => a.Materia).ToList();
        }
    }
}