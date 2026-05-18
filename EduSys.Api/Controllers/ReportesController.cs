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
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(
            IHorarioRepository horarioRepo,
            IReportesRepository reporteRepo,
            IAlumnoRepository alumnoRepo,
            IPdfReportService pdfService,
            IWebHostEnvironment env,
            ILogger<ReportesController> logger)
        {
            _horarioRepo = horarioRepo;
            _reporteRepo = reporteRepo;
            _alumnoRepo = alumnoRepo;
            _pdfService = pdfService;
            _env = env;
            _logger = logger;
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

        [HttpGet("certificado-regular")]
        [Produces("application/pdf")]
        [Authorize(Roles = "Alumno, REP_VER")]
        public async Task<IActionResult> DescargarCertificadoRegular([FromQuery] int idAlumno, [FromQuery] int idPeriodo)
        {
            try
            {
                // 1. Validamos en base de datos si el alumno es regular
                var datos = await _reporteRepo.GetDatosCertificadoRegularAsync(idAlumno, idPeriodo);

                // Si retorna null, es porque no tiene materias anotadas
                if (datos == null)
                    return BadRequest(new { message = "No se registran inscripciones activas para este ciclo lectivo." });

                // 2. Generamos el PDF
                byte[] pdfBytes = _pdfService.GenerarCertificadoAlumnoRegular(datos);

                // 3. Devolvemos el archivo
                return File(pdfBytes, "application/pdf", $"Certificado_Regular_{datos.Legajo}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el servidor al generar Certificado Regular.");
                return StatusCode(500, new { message = "Ocurrió un error inesperado en el servidor al generar el documento." });
            }
        }


        private Document CrearHorarioPdf(HorarioRequestDTO request)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    // 1. A4 Horizontal con márgenes de 10pt (Espacio máximo)
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(10);
                    page.PageColor(Colors.White);
                    // Interlineado global al 0.9 para aplastar los textos y ahorrar altura
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).LineHeight(0.9f));

                    // 2. Encabezado institucional ultra-compacto
                    page.Header().Column(col =>
                    {
                        col.Item().Text("CRONOGRAMA DE HORARIOS").FontColor(P.Teal).FontSize(8).Bold();
                        col.Item().Text(request.CarreraNombre.ToUpper()).FontColor(P.Blue).FontSize(12).Bold();

                        col.Item().PaddingTop(2).Background(P.Blue).Padding(2).AlignCenter()
                            .Text($"{request.Periodo.ToUpper()} — {request.SedeNombre.ToUpper()}")
                            .FontColor(Colors.White).FontSize(9).Bold();
                    });

                    // 3. Contenido (Tabla protegida con ScaleToFit)
                    page.Content().PaddingTop(5).ScaleToFit().Table(table =>
                    {
                        var diasSemanales = new List<string> { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
                        if (request.Horarios.Any(h => h.Dia.Equals("Sábado", StringComparison.OrdinalIgnoreCase)))
                            diasSemanales.Add("Sábado");

                        // Columnas fijas muy ajustadas
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25); // Año
                            columns.ConstantColumn(40); // Comisión
                            foreach (var _ in diasSemanales) columns.RelativeColumn();
                        });

                        // Cabeceras de Tabla
                        table.Cell().RowSpan(1).ColumnSpan(2).BorderBottom(1).BorderColor(P.Blue);

                        foreach (var dia in diasSemanales)
                        {
                            table.Cell().Background("#d97706").Border(1).BorderColor(P.Blue)
                                 .Padding(2).AlignCenter().AlignMiddle()
                                 .Text(dia.ToUpper()).FontColor(Colors.White).Bold().FontSize(8);
                        }

                        // Agrupación
                        var gruposAnio = request.Horarios.GroupBy(h => h.AnioCursada).OrderBy(g => g.Key).ToList();

                        foreach (var gAnio in gruposAnio)
                        {
                            var comisiones = gAnio.GroupBy(h => h.ComisionCodigo).OrderBy(g => g.Key).ToList();
                            bool primeraComision = true;

                            foreach (var gCom in comisiones)
                            {
                                // Celda combinada del Año
                                if (primeraComision)
                                {
                                    table.Cell().RowSpan((uint)comisiones.Count)
                                         .Background("#fffbeb").Border(1).BorderColor(P.Blue)
                                         .AlignCenter().AlignMiddle()
                                         .Text($"{gAnio.Key}º\nAÑO").FontColor("#d97706").Bold().FontSize(9);
                                    primeraComision = false;
                                }

                                // 🚀 Celda de Comisión (Agregamos la palabra "Comisión")
                                table.Cell().Background(P.RowAlt).Border(1).BorderColor(P.Blue)
                                     .AlignCenter().AlignMiddle().Padding(1)
                                     .Text(t =>
                                     {
                                         t.Span("Comisión\n").FontSize(5).FontColor(P.TextSoft);
                                         t.Span(gCom.Key).FontColor(P.Blue).Bold().FontSize(8);
                                     });

                                // Celdas de los Días
                                foreach (var dia in diasSemanales)
                                {
                                    var clases = gCom.Where(h => h.Dia.Equals(dia, StringComparison.OrdinalIgnoreCase))
                                                     .OrderBy(h => h.HoraInicio).ToList();

                                    var cell = table.Cell().Border(1).BorderColor(P.Blue).Padding(1).AlignMiddle();

                                    if (clases.Any())
                                    {
                                        cell.Column(col =>
                                        {
                                            foreach (var clase in clases)
                                            {
                                                var profTexto = string.IsNullOrWhiteSpace(clase.Profesor) ? "A designar" : clase.Profesor;
                                                var aulaTexto = string.IsNullOrWhiteSpace(clase.Aula) ? "A Conf." : clase.Aula.ToUpper();
                                                var codTexto = string.IsNullOrWhiteSpace(clase.Codigo) ? "" : $"[{clase.Codigo}] ";

                                                col.Item().AlignCenter().Text(t =>
                                                {
                                                    t.Span($"{clase.HoraInicio:hh\\:mm} a {clase.HoraFin:hh\\:mm} hs\n").FontColor(P.TextSoft).Bold().FontSize(6);
                                                    t.Span($"{codTexto}{clase.Materia.ToUpper()}\n").FontColor(P.Blue).Bold().FontSize(6);
                                                    t.Span($"Prof: {profTexto} | Aula: {aulaTexto}").FontColor("#d97706").Bold().FontSize(5);
                                                });

                                                if (clase != clases.Last())
                                                {
                                                    col.Item().PaddingVertical(1).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                                                }
                                            }
                                        });
                                    }
                                    else
                                    {
                                        cell.Text(""); // Día libre
                                    }
                                }
                            }
                        }
                    });

                    // 4. 🚀 Pie de página con Firma y Sello
                    page.Footer().PaddingTop(10).Row(row =>
                    {
                        // Texto legal a la izquierda
                        row.RelativeItem().AlignBottom().Text(x =>
                        {
                            x.Span("Cronograma oficial de horarios - Sistema EduSys").FontSize(7).FontColor(P.TextSoft);
                        });

                        // Bloque de Firma y Sello a la derecha
                        row.ConstantItem(120).Column(c =>
                        {
                            try
                            {
                                string imagePath = Path.Combine(_env.WebRootPath, "images", "sello_edusys.png");
                                if (System.IO.File.Exists(imagePath))
                                {
                                    c.Item().AlignCenter().Height(35).Image(imagePath);
                                }
                                else { c.Item().Height(35); } // Espacio en blanco si no encuentra la imagen
                            }
                            catch { c.Item().Height(35); }

                            // Si no usas imagen, dejamos el espacio en blanco para que firmen a mano
                            c.Item().Height(35);

                            c.Item().LineHorizontal(0.5f).LineColor(P.BlueDark);
                            c.Item().AlignCenter().Text("Firma y Sello Autorizado").FontSize(6).FontColor(P.TextSoft);
                            c.Item().AlignCenter().Text("Secretaría Académica").FontSize(7).Bold().FontColor(P.Blue);
                        });
                    });
                });
            });
        }
    }
}