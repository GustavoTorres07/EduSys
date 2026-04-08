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
    // 🔓 Candado Base: Requiere autenticación
    [Authorize]
    public class ReportesController : ControllerBase
    {
        // ================================================================
        // PALETA OFICIAL EDUSYS
        // ================================================================
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
        // ENDPOINTS
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

        [HttpGet("certificado-alumno-regular-descargar")]
        [Produces("application/pdf")]
        [Authorize(Roles = "Alumno, REP_VER")]
        public async Task<IActionResult> DescargarCertificadoRegular(
            [FromQuery] int idAlumno, [FromQuery] int idPeriodo)
        {
            try
            {
                var datos = await _reporteRepo.GetDatosCertificadoRegularAsync(idAlumno, idPeriodo);
                if (datos == null)
                    return BadRequest(new { message = "Sin inscripciones activas." });

                QuestPDF.Settings.License = LicenseType.Community;
                byte[] pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x =>
                            x.FontSize(10).FontFamily("Arial").FontColor(P.Text));
                        page.Header().Element(c => HeaderCertificadoRegular(c, datos));
                        page.Content().PaddingVertical(10).Element(c => BodyCertificadoRegular(c, datos));
                        page.Footer().Element(FooterComun);
                    });
                }).GeneratePdf();

                return File(pdfBytes, "application/pdf",
                    $"Certificado_Regular_{datos.Legajo}_{DateTime.Now:yyyyMMdd}.pdf");
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
        // PDF PRINCIPAL - ALINEACIÓN SUPERIOR (CERO ESPACIOS RAROS)
        // ================================================================

        private Document CrearHorarioPdf(HorarioRequestDTO request)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    // 🚀 A4 HORIZONTAL, márgenes de 10 puntos para ocupar toda la hoja
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(10, Unit.Point);
                    page.PageColor(Colors.White);

                    // Interlineado hiper eficiente (0.9f)
                    page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Arial").FontColor(P.Text).LineHeight(0.9f));

                    var comisiones = request.Horarios
                        .Select(h => new { Anio = h.AnioCursada, Com = h.ComisionCodigo ?? "S/C" })
                        .Distinct()
                        .OrderBy(c => c.Anio).ThenBy(c => c.Com)
                        .Select(c => new {
                            Display = $"{c.Anio}° {c.Com}".Trim(),
                            c.Anio,
                            c.Com
                        }).ToList();

                    var diasPosibles = new[] { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado" };

                    string sedeLimpia = (request.SedeNombre ?? "Central").Replace("Sede ", "", StringComparison.OrdinalIgnoreCase);

                    // ── HEADER ──────────────────────────
                    page.Header().PaddingBottom(4).Row(row =>
                    {
                        row.RelativeItem().Text(txt =>
                        {
                            txt.Span(request.CarreraNombre.ToUpper()).FontSize(11).Bold().FontColor(P.BlueDark);
                            txt.Span($"   |   {request.Periodo} - SEDE {sedeLimpia.ToUpper()}").FontSize(9).FontColor(P.TextMid);
                        });
                        row.ConstantItem(120).AlignRight().Text($"Emitido: {DateTime.Now:dd/MM/yyyy}").FontSize(7.5f).FontColor(P.TextSoft);
                    });

                    // ── TABLA MATRICIAL ──────────────────────────────
                    page.Content().Table(t =>
                    {
                        t.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(18); // Día rotado
                            foreach (var _ in comisiones) cols.RelativeColumn();
                        });

                        // CABECERA
                        t.Header(h =>
                        {
                            h.Cell().Border(0.5f).BorderColor(P.BorderDark).Background(P.BlueDark);

                            foreach (var c in comisiones)
                            {
                                h.Cell().Border(0.5f).BorderColor(P.BorderDark).Background(P.BlueDark)
                                 .PaddingVertical(3).AlignCenter().AlignMiddle()
                                 .Text(c.Display).FontSize(9.5f).Bold().FontColor(P.White);
                            }
                        });

                        // FILAS POR DÍA
                        int rowIndex = 0;
                        foreach (var dia in diasPosibles)
                        {
                            var clasesDiaGlobal = request.Horarios
                                .Where(h => NormalizarDia(h.Dia) == NormalizarDia(dia))
                                .ToList();

                            if (!clasesDiaGlobal.Any()) continue;

                            string rowBgColor = (rowIndex % 2 == 0) ? P.Surface : P.RowAlt;
                            rowIndex++;

                            // DÍA ROTADO
                            t.Cell().Border(0.5f).BorderColor(P.BorderDark).Background(P.BlueDark)
                             .AlignCenter().AlignMiddle()
                             .RotateLeft()
                             .Text(dia.ToUpper()).FontSize(8.5f).Bold().FontColor(P.White);

                            // CELDAS DE MATERIAS
                            foreach (var c in comisiones)
                            {
                                var clasesComisionDia = clasesDiaGlobal
                                    .Where(h => h.AnioCursada == c.Anio && (h.ComisionCodigo ?? "S/C") == c.Com)
                                    .OrderBy(h => h.HoraInicio)
                                    .ToList();

                                // ✨ AQUÍ ESTÁ EL ARREGLO: Se usa AlignTop() en lugar de AlignMiddle() o ExtendVertical()
                                // Esto asegura que todas las celdas comiencen exactamente desde la línea superior, prolijas.
                                var cell = t.Cell().Border(0.5f).BorderColor(P.BorderDark).Background(rowBgColor)
                                            .PaddingVertical(2).PaddingHorizontal(4).AlignTop();

                                if (!clasesComisionDia.Any())
                                {
                                    cell.AlignCenter().Text("").FontSize(8);
                                }
                                else
                                {
                                    cell.Column(col =>
                                    {
                                        for (int i = 0; i < clasesComisionDia.Count; i++)
                                        {
                                            var clase = clasesComisionDia[i];

                                            col.Item().Column(inner =>
                                            {
                                                inner.Spacing(0);

                                                // 1. Horario
                                                inner.Item().AlignLeft().Text($"{clase.HoraInicio:hh\\:mm} a {clase.HoraFin:hh\\:mm} hs")
                                                     .FontSize(8.5f).Bold().FontColor(P.Teal);

                                                // 2. Código y Materia
                                                inner.Item().AlignLeft().Text(txt =>
                                                {
                                                    if (!string.IsNullOrWhiteSpace(clase.Codigo))
                                                        txt.Span($"[{clase.Codigo.ToUpper()}] ").FontSize(7.5f).FontColor(P.TextMid);

                                                    txt.Span((clase.Materia ?? "S/D").ToUpper()).FontSize(8.5f).Bold().FontColor(P.Text);
                                                });

                                                // 3. Aula y Profesor
                                                string aula = string.IsNullOrWhiteSpace(clase.Aula) ? "A Confirmar" : clase.Aula;
                                                string prof = string.IsNullOrWhiteSpace(clase.Profesor) ? "A designar" : clase.Profesor;

                                                inner.Item().AlignLeft()
                                                     .Text($"Aula: {aula} | Prof: {prof}")
                                                     .FontSize(7.5f).Italic().FontColor(P.TextSoft);
                                            });

                                            // Divisor
                                            if (i < clasesComisionDia.Count - 1)
                                            {
                                                col.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(P.Border);
                                            }
                                        }
                                    });
                                }
                            }
                        }
                    });

                    // ── FOOTER ───────────────────────────────────────────────
                    page.Footer().PaddingTop(2).Row(row =>
                    {
                        row.RelativeItem()
                           .Text("EduSys Académico - Documentación Oficial")
                           .FontSize(6.5f).FontColor(P.TextSoft);
                    });
                });
            });
        }

        // ================================================================
        // UTILS Y OTROS REPORTES
        // ================================================================

        private static string NormalizarDia(string dia)
        {
            if (string.IsNullOrWhiteSpace(dia)) return dia ?? "";
            return dia.ToLower()
                .Replace("é", "e").Replace("É", "E")
                .Replace("á", "a").Replace("Á", "A")
                .Replace("ó", "o").Replace("Ó", "O")
                .Replace("í", "i").Replace("Í", "I")
                .Replace("ú", "u").Replace("Ú", "U");
        }

        private void HeaderCertificadoRegular(IContainer container, CertificadoAlumnoRegularDTO data)
        {
            container.Column(col =>
            {
                col.Item().Height(3).Background(P.Blue);
                col.Item()
                   .Background(P.Surface).BorderBottom(1).BorderColor(P.Border)
                   .PaddingHorizontal(14).PaddingVertical(10)
                   .Row(row =>
                   {
                       row.RelativeItem().Column(c =>
                       {
                           c.Item().Text(data.InstitucionNombre.ToUpper())
                                   .FontSize(11).Bold().FontColor(P.Blue);
                           c.Item().Text(data.Departamento)
                                   .FontSize(7.5f).FontColor(P.TextSoft);
                       });
                       row.RelativeItem().Column(c =>
                       {
                           c.Item().AlignRight()
                            .Text("CERTIFICADO DE ALUMNO REGULAR")
                            .FontSize(10).Bold().Underline().FontColor(P.Text);
                           c.Item().AlignRight()
                            .Text($"Ciclo Lectivo: {data.PeriodoAcademico}")
                            .FontSize(8).FontColor(P.TextMid);
                           c.Item().AlignRight()
                            .Text($"Emisión: {data.FechaEmision:dd/MM/yyyy}")
                            .FontSize(7.5f).Italic().FontColor(P.TextSoft);
                       });
                   });
            });
        }

        private void BodyCertificadoRegular(IContainer container, CertificadoAlumnoRegularDTO data)
        {
            container.Column(col =>
            {
                col.Item().PaddingTop(28).Text(text =>
                {
                    text.ParagraphSpacing(8);
                    text.Span("La autoridad que suscribe certifica que el/la alumno/a ")
                        .FontSize(9.5f).FontColor(P.TextMid);
                    text.Span(data.AlumnoNombre.ToUpper())
                        .FontSize(9.5f).SemiBold().FontColor(P.Text);
                    text.Span(", DNI N° ").FontSize(9.5f).FontColor(P.TextMid);
                    text.Span(data.Dni).FontSize(9.5f).SemiBold();
                    text.Span(" y Legajo N° ").FontSize(9.5f).FontColor(P.TextMid);
                    text.Span(data.Legajo).FontSize(9.5f).SemiBold();
                    text.Span(", es alumno/a regular de la carrera ")
                        .FontSize(9.5f).FontColor(P.TextMid);
                    text.Span(data.Carrera.ToUpper())
                        .FontSize(9.5f).SemiBold().FontColor(P.Text);
                    text.Span($", que se dicta en la Sede {data.Sede}, durante el Ciclo Lectivo {data.PeriodoAcademico}.")
                        .FontSize(9.5f).FontColor(P.TextMid);
                });

                col.Item().PaddingVertical(18)
                   .Background(P.BluePale).Border(1).BorderColor(P.Border)
                   .BorderLeft(3).BorderColor(P.Blue).Padding(14)
                   .Row(row =>
                   {
                       row.RelativeItem().Column(c =>
                       {
                           c.Item().Text("ESTADO ACADÉMICO")
                                   .FontSize(7).FontColor(P.TextSoft).SemiBold();
                           c.Item().PaddingTop(3).Text("REGULAR")
                                   .FontSize(13).Bold().FontColor(P.Teal);
                       });
                       row.RelativeItem().Column(c =>
                       {
                           c.Item().Text("PERÍODO / CICLO LECTIVO")
                                   .FontSize(7).FontColor(P.TextSoft).SemiBold();
                           c.Item().PaddingTop(3).Text(data.PeriodoAcademico)
                                   .FontSize(11).SemiBold().FontColor(P.Text);
                       });
                   });

                col.Item().PaddingTop(10).Text(text =>
                {
                    text.Span("Se extiende el presente certificado a pedido del interesado/a en la ciudad de ")
                        .FontSize(9).FontColor(P.TextMid);
                    text.Span(data.Ciudad).FontSize(9).SemiBold().FontColor(P.Text);
                    text.Span(", Provincia de ").FontSize(9).FontColor(P.TextMid);
                    text.Span(data.Provincia).FontSize(9).SemiBold().FontColor(P.Text);
                    text.Span($", a los {data.FechaEmision.Day} días del mes de {data.FechaEmision:MMMM} del año {data.FechaEmision.Year}.")
                        .FontSize(9).FontColor(P.TextMid);
                });

                col.Item().PaddingTop(32).AlignRight().Row(r =>
                {
                    var rutaSello = ObtenerRutaSelloLocal();
                    if (System.IO.File.Exists(rutaSello))
                    {
                        r.ConstantItem(70).Image(System.IO.File.ReadAllBytes(rutaSello)).FitWidth();
                        r.ConstantItem(10);
                    }
                    r.AutoItem().Column(firma =>
                    {
                        firma.Item().AlignRight()
                             .Text("_______________________________").FontColor(P.Border);
                        firma.Item().PaddingTop(3).AlignRight()
                             .Text(data.RectorNombre).FontSize(8.5f).SemiBold().FontColor(P.TextMid);
                        firma.Item().AlignRight()
                             .Text(data.RectorCargo).FontSize(8).Italic().FontColor(P.TextSoft);
                    });
                });
            });
        }

        private void FooterComun(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(P.Border);
                col.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem()
                       .Text("Documento generado automáticamente · EduSys")
                       .FontSize(7).FontColor(P.TextSoft);
                    row.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Página ").FontSize(7).FontColor(P.TextSoft);
                        x.CurrentPageNumber().FontSize(7).FontColor(P.TextMid).SemiBold();
                        x.Span(" de ").FontSize(7).FontColor(P.TextSoft);
                        x.TotalPages().FontSize(7).FontColor(P.TextMid).SemiBold();
                    });
                });
            });
        }

        private string ObtenerNombrePeriodoLocal(int idPeriodo) => $"Periodo {idPeriodo}";

        private string ObtenerRutaSelloLocal()
        {
            var ruta = Path.Combine(_env.WebRootPath ?? "wwwroot", "images", "sello_edusys.png");
            if (!System.IO.File.Exists(ruta))
                ruta = Path.Combine(AppContext.BaseDirectory, "wwwroot", "images", "sello_edusys.png");
            return ruta;
        }
    }
}