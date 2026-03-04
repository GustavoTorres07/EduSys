using EduSys.Api.Repositories.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduSys.Shared.Models;
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
        public async Task<ActionResult<List<InscripcionCursadaListadoDTO>>> GetPorAlumno(int idAlumno, int idPeriodo)
        {
            var lista = await _inscripcionRepository.GetInscripcionesPorAlumnoAsync(idAlumno, idPeriodo);

            var result = lista.Select(i => new InscripcionCursadaListadoDTO
            {
                IdInscripcion = i.Id,
                // Agregamos "?" para evitar crasheos si falta alguna relación
                Materia = i.IdComisionNavigation?.IdPlanMateriaNavigation?.IdMateriaNavigation?.Nombre ?? "Materia sin nombre",
                ComisionCodigo = i.IdComisionNavigation?.Codigo ?? "S/C",
                Estado = i.Estado,
                Fecha = i.FechaInscripcion ?? DateTime.Now
            }).ToList();

            return Ok(result);
        }

        // =========================================================================
        // 5. INSCRIBIR ADMIN (Con Overrides / Excepciones)
        // =========================================================================
        [HttpPost("admin/inscribir")]
        [Authorize(Roles = "Administrador, Secretaria Academica")] // Protegido por rol
        public async Task<ActionResult<ResultadoInscripcionDTO>> InscribirManual([FromBody] InscripcionManualDTO dto)
        {
            try
            {
                // Llamamos al método especial que creamos en el repositorio
                var resultado = await _inscripcionRepository.InscribirAdminAsync(dto);

                if (resultado.Exito)
                    return Ok(resultado);
                else
                    return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInscripcionDTO
                {
                    Exito = false,
                    Mensaje = "Error interno al forzar inscripción: " + ex.Message
                });
            }
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

        [HttpGet("admin/alumno/{idAlumno}")]
        [Authorize(Roles = "Administrador, Secretaria Academica")]
        public async Task<ActionResult<List<InscripcionCursadaListadoDTO>>> GetInscripcionesAlumno(int idAlumno)
        {
            var inscripciones = await _inscripcionRepository.GetInscripcionesByAlumnoAsync(idAlumno);
            return Ok(inscripciones);
        }
    }
}