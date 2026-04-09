using EduSys.Api.Repositories.Interfaces;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;

using Document = QuestPDF.Fluent.Document;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace EduSys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private static class P
        {
            public const string Blue = "#1e3a58";
            public const string BlueDark = "#0f172a";
            public const string BluePale = "#e0f2fe";
            public const string Teal = "#0284c7";
            public const string Surface = "#ffffff";
            public const string Border = "#cbd5e1";
            public const string BorderDark = "#94a3b8";
            public const string RowAlt = "#f8fafc";
            public const string Text = "#0f172a";
            public const string TextMid = "#475569";
            public const string TextSoft = "#64748b";
            public const string White = "#ffffff";
        }

        private readonly IHorarioRepository _horarioRepo;
        private readonly IReportesRepository _reporteRepo;
        private readonly IAlumnoRepository _alumnoRepo;
        private readonly IPdfReportService _pdfService;
        private readonly IWebHostEnvironment _env;

        public ReportesController(
            IHorarioRepository horarioRepo,
            IReportesRepository reporteRepo,
            IAlumnoRepository alumnoRepo,
            IPdfReportService pdfService,
            IWebHostEnvironment env)
        {
            _horarioRepo = horarioRepo;
            _reporteRepo = reporteRepo;
            _alumnoRepo = alumnoRepo;
            _pdfService = pdfService;
            _env = env;
        }

        // ================================================================
        // 🚀 NUEVO ENDPOINT PARA LAS ACTAS INDIVIDUALES
        // ================================================================
        [HttpGet("acta-individual/{idActa}")]
        [Authorize(Roles = "ACTA_VER, Administrador, Docente, Alumno")]
        [Produces("application/pdf")]
        public async Task<IActionResult> DescargarActaIndividual(int idActa)
        {
            try
            {
                var datos = await _reporteRepo.GetDatosActaIndividualAsync(idActa);

                if (datos == null)
                    return NotFound(new { message = "El acta solicitada no existe." });

                byte[] pdfBytes = _pdfService.GenerarActaIndividual(datos);

                return File(pdfBytes, "application/pdf", $"Acta_{datos.NumeroActa}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al generar el acta: {ex.Message}" });
            }
        }

        // ================================================================
        // (TUS OTROS ENDPOINTS SIGUEN EXACTAMENTE IGUAL)
        // ================================================================

        [HttpGet("horarios-alumno-cursando")]
        [Authorize(Roles = "Alumno, REP_VER")]
        public async Task<IActionResult> GetHorariosCursando([FromQuery] int idPeriodo, [FromQuery] int idAlumno)
        {
            try
            {
                var h = await _horarioRepo.GetHorariosCursandoAsync(idPeriodo, idAlumno);
                return Ok(h ?? new List<HorarioVisualizacionDTO>());
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("horario-descargar")]
        [Produces("application/pdf")]
        public async Task<IActionResult> DescargarHorarioGet(
            [FromQuery] int idPeriodo,
            [FromQuery] int idCarrera,
            [FromQuery] int idSede)
        {
            try
            {
                var horarios = await _horarioRepo.GetHorariosByCarreraAndPeriodoAsync(
                    idPeriodo, idCarrera, idSede);

                if (!horarios.Any())
                    return NotFound(new { message = "No se encontraron horarios para la carrera y sede seleccionada." });

                foreach (var h in horarios)
                {
                    h.Curso ??= h.ComisionCodigo ?? "—";
                    h.Materia ??= "Materia no informada";
                }

                var first = horarios.First();
                var request = new HorarioRequestDTO
                {
                    CarreraNombre = first.CarreraNombre ?? "Carrera",
                    SedeNombre = first.Sede ?? "Sede Central",
                    Periodo = first.PeriodoNombre ?? $"Periodo {idPeriodo}",
                    Horarios = horarios
                };

                QuestPDF.Settings.License = LicenseType.Community;
                byte[] pdfBytes = CrearHorarioPdf(request).GeneratePdf();

                return File(pdfBytes, "application/pdf", $"Horarios_{request.CarreraNombre.Replace(" ", "_")}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al generar el PDF: {ex.Message}", detail = ex.StackTrace });
            }
        }

        [HttpGet("constancia-inscripcion")]
        [Produces("application/pdf")]
        [Authorize(Roles = "Alumno, REP_VER")]
        public async Task<IActionResult> DescargarConstancia(
            [FromQuery] int idAlumno, [FromQuery] int idPeriodo)
        {
            try
            {
                var datos = await _reporteRepo.GetDatosConstanciaAsync(idAlumno, idPeriodo);
                if (datos == null || !datos.Materias.Any())
                    return BadRequest(new { message = "Sin materias inscriptas." });

                byte[] pdfBytes = _pdfService.GenerarConstanciaInscripcion(datos);
                return File(pdfBytes, "application/pdf",
                    $"Constancia_Cursada_{datos.Legajo}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("historia-academica")]
        [Authorize(Roles = "Alumno, REP_VER, ALU_ABM")]
        public async Task<IActionResult> GetHistoriaAcademica([FromQuery] int idAlumno)
        {
            try
            {
                var h = await _reporteRepo.GetHistoriaAcademicaAsync(idAlumno);
                if (h == null) return NotFound(new { message = "Alumno no encontrado." });
                return Ok(h);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("analitico-provisorio")]
        [Authorize(Roles = "Alumno")]
        [Produces("application/pdf")]
        public async Task<IActionResult> DescargarAnaliticoProvisorio()
        {
            try
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(claim, out int idUsuario))
                    return Unauthorized(new { message = "Token inválido." });

                var alumno = await _alumnoRepo.GetByUsuarioAsync(idUsuario);
                if (alumno == null)
                    return Unauthorized(new { message = "Perfil no encontrado." });

                var historia = await _reporteRepo.GetHistoriaAcademicaAsync(alumno.Id);
                if (historia == null || !historia.Detalle.Any())
                    return BadRequest(new { message = "Sin historial registrado." });

                byte[] pdfBytes = _pdfService.GenerarAnaliticoProvisorio(historia);
                return File(pdfBytes, "application/pdf",
                    $"Analitico_{historia.Legajo}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("constancia-final/{idInscripcion}")]
        [Authorize(Roles = "Alumno")]
        [Produces("application/pdf")]
        public async Task<IActionResult> DescargarConstanciaFinal(int idInscripcion)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int idUsuario))
                return Unauthorized(new { message = "Token inválido." });

            var alumno = await _alumnoRepo.GetByUsuarioAsync(idUsuario);
            if (alumno == null)
                return Unauthorized(new { message = "Perfil no encontrado." });

            var datos = await _reporteRepo.GetDatosConstanciaFinalAsync(idInscripcion, alumno.Id);
            if (datos == null)
                return NotFound(new { message = "Inscripción no encontrada." });

            byte[] pdfBytes = _pdfService.GenerarConstanciaInscripcionFinal(datos);
            return File(pdfBytes, "application/pdf",
                $"Constancia_Final_{datos.MateriaNombre.Replace(" ", "_")}.pdf");
        }

        // ================================================================
        // PDF PRINCIPAL - UTILS
        // ================================================================
        private Document CrearHorarioPdf(HorarioRequestDTO request)
        {
            // Tu código original de CrearHorarioPdf...
            return Document.Create(container => container.Page(page => page.Content().Text("...")));
        }
    }
}