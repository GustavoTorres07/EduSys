using EduSys.Api.Data;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSys.Api.Repositories
{
    public class AsistenciaRepository : IAsistenciaRepository
    {
        private readonly EduSysDbContext _context;
        private readonly ILogger<AsistenciaRepository> _logger;

        public AsistenciaRepository(EduSysDbContext context, ILogger<AsistenciaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AsistenciaGrillaDTO> GetGrillaByComisionAsync(int idComision)
        {
            try
            {
                // 1. Obtener alumnos inscriptos en esta comisión (usando los nombres de navegación reales)
                var inscripciones = await _context.InscripcionCursada
                    .Include(i => i.IdAlumnoNavigation)
                        .ThenInclude(a => a.IdUsuarioNavigation)
                    .Where(i => i.IdComision == idComision && i.Estado != "Baja")
                    .ToListAsync();

                var idsInscripciones = inscripciones.Select(i => i.Id).ToList();

                // 2. Obtener todas las asistencias registradas para estas inscripciones
                var asistenciasBd = await _context.Asistencia
                    .Where(a => idsInscripciones.Contains(a.IdInscripcionCursada))
                    .ToListAsync();

                // 3. Extraer las fechas únicas de las clases dictadas (Pasando de DateOnly a DateTime)
                var fechas = asistenciasBd
                    .Select(a => a.Fecha.ToDateTime(TimeOnly.MinValue))
                    .Distinct()
                    .OrderBy(f => f)
                    .ToList();

                // 4. Armar el DTO
                var grilla = new AsistenciaGrillaDTO
                {
                    Fechas = fechas,
                    Alumnos = inscripciones.Select(i => new AlumnoAsistenciaFilaDTO
                    {
                        IdInscripcionCursada = i.Id,
                        NombreCompleto = $"{i.IdAlumnoNavigation.IdUsuarioNavigation.Apellido}, {i.IdAlumnoNavigation.IdUsuarioNavigation.Nombre}",
                        Legajo = i.IdAlumnoNavigation.Legajo,
                        Asistencias = asistenciasBd
                            .Where(a => a.IdInscripcionCursada == i.Id)
                            .Select(a => new AsistenciaDetalleDTO
                            {
                                Id = a.Id,
                                IdInscripcionCursada = a.IdInscripcionCursada,
                                // Pasando de DateOnly a DateTime para el Frontend
                                Fecha = a.Fecha.ToDateTime(TimeOnly.MinValue),
                                EstaPresente = a.EstaPresente,
                                EsJustificado = a.EsJustificado,
                                Observacion = a.Observacion,
                                UrlCertificado = a.UrlCertificado,
                                Registrado = true
                            }).ToList()
                    }).OrderBy(a => a.NombreCompleto).ToList()
                };

                return grilla;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar la grilla de asistencia para la comisión {IdComision}", idComision);
                throw;
            }
        }

        public async Task<bool> GuardarGrillaAsync(GuardarAsistenciaRequestDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var dto in request.Asistencias)
                {
                    if (dto.Id == 0) // Es un registro nuevo
                    {
                        var nuevaAsistencia = new Asistencia
                        {
                            IdInscripcionCursada = dto.IdInscripcionCursada,
                            // Pasando de DateTime (Frontend) a DateOnly (SQL Database)
                            Fecha = DateOnly.FromDateTime(dto.Fecha),
                            EstaPresente = dto.EstaPresente,
                            EsJustificado = dto.EsJustificado,
                            Observacion = dto.Observacion,
                            UrlCertificado = dto.UrlCertificado
                        };
                        await _context.Asistencia.AddAsync(nuevaAsistencia);
                    }
                    else // Actualizar registro existente
                    {
                        var asistenciaExistente = await _context.Asistencia.FindAsync(dto.Id);
                        if (asistenciaExistente != null)
                        {
                            asistenciaExistente.EstaPresente = dto.EstaPresente;
                            asistenciaExistente.EsJustificado = dto.EsJustificado;
                            asistenciaExistente.Observacion = dto.Observacion;

                            // Solo actualiza la URL si viene una nueva, para no borrar la anterior por error
                            if (!string.IsNullOrWhiteSpace(dto.UrlCertificado))
                            {
                                asistenciaExistente.UrlCertificado = dto.UrlCertificado;
                            }

                            _context.Asistencia.Update(asistenciaExistente);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al guardar la asistencia masiva para la comisión {IdComision}", request.IdComision);
                return false;
            }
        }
    }
}