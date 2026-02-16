using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere estar logueado
    public class InscripcionesController : ControllerBase
    {
        private readonly IInscripcionRepository _inscripcionRepository;

        public InscripcionesController(IInscripcionRepository inscripcionRepository)
        {
            _inscripcionRepository = inscripcionRepository;
        }

        // =========================================================================
        // 1. INSCRIBIRSE (Acción principal)
        // =========================================================================
        [HttpPost("inscribir")]
        public async Task<ActionResult<ResultadoInscripcionDTO>> Inscribir(InscripcionCursadaRequestDTO dto)
        {
            try
            {
                // ✅ CORRECCIÓN: Pasamos el objeto 'dto' completo.
                // Asegúrate que tu interfaz IInscripcionRepository acepte (InscripcionCursadaRequestDTO dto)
                var resultado = await _inscripcionRepository.InscribirAlumnoAsync(dto);

                if (resultado.Exito)
                    return Ok(resultado);
                else
                    return BadRequest(resultado); // Devuelve error de validación (ej: cupo, correlativa)
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInscripcionDTO
                {
                    Exito = false,
                    Mensaje = "Error interno al procesar la inscripción: " + ex.Message
                });
            }
        }

        // =========================================================================
        // 2. VER OFERTA (Para que el alumno elija)
        // =========================================================================
        [HttpGet("oferta/{idAlumno}")]
        public async Task<ActionResult<List<ComisionDTO>>> GetOferta(int idAlumno, [FromQuery] int idPeriodo)
        {
            var oferta = await _inscripcionRepository.GetOfertaParaAlumnoAsync(idAlumno, idPeriodo);
            return Ok(oferta);
        }

        // =========================================================================
        // 3. VER MIS INSCRIPCIONES (Para que el alumno vea qué cursa)
        // =========================================================================
        [HttpGet("alumno/{idAlumno}/periodo/{idPeriodo}")]
        public async Task<ActionResult> GetPorAlumno(int idAlumno, int idPeriodo)
        {
            var lista = await _inscripcionRepository.GetInscripcionesPorAlumnoAsync(idAlumno, idPeriodo);

            // Mapeo simple para la vista del alumno
            var result = lista.Select(i => new
            {
                i.Id,
                Materia = i.IdComisionNavigation.IdPlanMateriaNavigation.IdMateriaNavigation.Nombre,
                Comision = i.IdComisionNavigation.Codigo,
                Turno = i.IdComisionNavigation.Turno,
                Estado = i.Estado, // "Cursando", "Regular", etc.
                Fecha = i.FechaInscripcion
            });

            return Ok(result);
        }

        // =========================================================================
        // 4. DARSE DE BAJA
        // =========================================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> Cancelar(int id)
        {
            try
            {
                var exito = await _inscripcionRepository.CancelarInscripcionAsync(id);

                if (!exito)
                    return NotFound("Inscripción no encontrada o ya dada de baja.");

                return Ok(new { message = "Inscripción cancelada correctamente." });
            }
            catch (Exception ex)
            {
                // Captura excepciones de negocio (ej: periodo cerrado)
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("admin/inscribir")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // Seguridad RBAC
        public async Task<ActionResult<ResultadoInscripcionDTO>> InscribirManual([FromBody] InscripcionManualDTO dto)
        {
            // Llamamos al método especial de Admin
            var resultado = await _inscripcionRepository.InscribirAdminAsync(dto);

            if (resultado.Exito)
                return Ok(resultado);
            else
                return BadRequest(resultado);
        }
    }
}