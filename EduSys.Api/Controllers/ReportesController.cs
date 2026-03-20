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
        // ================================================================
        // PALETA OFICIAL EDUSYS
        // ================================================================
        private static class P
        {
            public const string Blue = "#456990";
            public const string BlueDark = "#344f6e";
            public const string BluePale = "#edf2f7";
            // Fondo alternado para separar comisiones visualmente
            public const string ComA = "#ffffff";   // Comisión impar  → blanco puro
            public const string ComB = "#f4f7fb";   // Comisión par    → gris-azulado muy suave
            public const string Teal = "#49BEAA";
            public const string Bg = "#f7f9fc";
            public const string Surface = "#ffffff";
            public const string Border = "#dde3ea";
            public const string BorderSep = "#b0c4d8";   // borde separador entre comisiones (más oscuro)
            public const string Text = "#1a2733";
            public const string TextMid = "#4a5568";
            public const string TextSoft = "#8a9ab0";
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
        public async Task<IActionResult> GetHorariosCursando(
            [FromQuery] int idPeriodo, [FromQuery] int idAlumno)
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
            var horarios = await _horarioRepo.GetHorariosByCarreraAndPeriodoAsync(
                idPeriodo, idCarrera, idSede);

            if (!horarios.Any())
                return NotFound(new { message = "No se encontraron horarios." });

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
                Periodo = ObtenerNombrePeriodoLocal(idPeriodo),
                Horarios = horarios
            };

            QuestPDF.Settings.License = LicenseType.Community;
            byte[] pdfBytes = CrearHorarioPdf(request).GeneratePdf();

            return File(pdfBytes, "application/pdf",
                $"Horario_{request.CarreraNombre.Replace(" ", "_")}.pdf");
        }

        [HttpGet("constancia-inscripcion")]
        [Produces("application/pdf")]
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
        // GENERADOR PDF HORARIO — UNA SOLA HOJA A4 LANDSCAPE
        // ================================================================
        private Document CrearHorarioPdf(HorarioRequestDTO request)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(6, Unit.Point);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(t =>
                        t.FontFamily("Arial").FontSize(6.5f).FontColor(P.Text));

                    // ── HEADER ────────────────────────────────────────────
                    page.Header().PaddingBottom(2).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item()
                                 .Text(request.CarreraNombre.ToUpper())
                                 .FontSize(10f).Bold().FontColor(P.BlueDark);
                                c.Item()
                                 .Text($"HORARIO DE CLASES  ·  {request.Periodo.ToUpper()}  ·  SEDE {request.SedeNombre?.ToUpper()}")
                                 .FontSize(6.5f).SemiBold().FontColor(P.TextSoft);
                            });
                            row.ConstantItem(100).AlignRight()
                               .Text($"Generado: {DateTime.Now:dd/MM/yyyy}")
                               .FontSize(6f).FontColor(P.TextSoft);
                        });
                        col.Item().PaddingTop(2).LineHorizontal(0.75f).LineColor(P.Border);
                    });

                    // ── TABLA ─────────────────────────────────────────────
                    page.Content().PaddingTop(3).Table(table =>
                    {
                        var dias = new List<string>
                            { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes" };

                        if (request.Horarios.Any(h =>
                                NormalizarDia(h.Dia).Equals("Sabado",
                                    StringComparison.OrdinalIgnoreCase)))
                            dias.Add("Sabado");

                        // ── COLUMNAS ──────────────────────────────────────
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(16);  // Año rotado
                            cols.ConstantColumn(34);  // Badge comisión
                            foreach (var _ in dias) cols.RelativeColumn();
                        });

                        // ── CABECERA DE DÍAS ──────────────────────────────
                        var nombresVisuales = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["Lunes"] = "L U N E S",
                            ["Martes"] = "M A R T E S",
                            ["Miercoles"] = "M I É R C O L E S",
                            ["Jueves"] = "J U E V E S",
                            ["Viernes"] = "V I E R N E S",
                            ["Sabado"] = "S Á B A D O",
                        };

                        table.Header(header =>
                        {
                            header.Cell().ColumnSpan(2)
                                  .Background(P.Bg)
                                  .BorderBottom(2).BorderColor(P.Blue);

                            foreach (var dia in dias)
                            {
                                string label = nombresVisuales.TryGetValue(dia, out var v)
                                    ? v : string.Join(" ", dia.ToUpper().ToCharArray());

                                header.Cell()
                                      .Background(P.BluePale)
                                      .BorderRight(1).BorderColor(P.Border)
                                      .BorderBottom(2).BorderColor(P.Blue)
                                      .PaddingVertical(5)
                                      .AlignCenter().AlignMiddle()
                                      .Text(label)
                                      .FontSize(6f).Bold().FontColor(P.Blue);
                            }
                        });

                        // ── FILAS ─────────────────────────────────────────
                        var gruposAnio = request.Horarios
                            .GroupBy(h => h.AnioCursada)
                            .OrderBy(g => g.Key)
                            .ToList();

                        bool isFirstAnio = true;

                        foreach (var gAnio in gruposAnio)
                        {
                            // Separador entre años — barra sólida 2 pt azul
                            if (!isFirstAnio)
                            {
                                table.Cell()
                                     .ColumnSpan((uint)(2 + dias.Count))
                                     .Height(2)
                                     .Background(P.Blue);
                            }
                            isFirstAnio = false;

                            var coms = gAnio
                                .GroupBy(h => h.ComisionCodigo ?? "S/C")
                                .OrderBy(g => g.Key)
                                .ToList();

                            uint rowSpanAnio = (uint)coms.Count;
                            bool isFirstCom = true;
                            int comIndex = 0; // para alternar fondo

                            foreach (var gCom in coms)
                            {
                                // Fondo alternado: blanco / gris-azulado suave
                                string bgRow = (comIndex % 2 == 0) ? P.ComA : P.ComB;
                                comIndex++;

                                // ── COLUMNA AÑO (rowspan) ─────────────────
                                if (isFirstCom)
                                {
                                    table.Cell()
                                         .RowSpan(rowSpanAnio)
                                         .Background(P.BluePale)
                                         .BorderRight(2).BorderColor(P.Blue)
                                         .AlignCenter().AlignMiddle()
                                         .RotateLeft()
                                         .PaddingVertical(1)
                                         .Text($"{gAnio.Key}° AÑO")
                                         .FontSize(6.5f).Bold().FontColor(P.Blue);

                                    isFirstCom = false;
                                }

                                // ── COLUMNA COMISIÓN ──────────────────────
                                // Borde superior más marcado en comisiones que no son la primera
                                // para separar visualmente dentro del mismo año
                                var comCell = table.Cell()
                                     .Background(P.Bg)
                                     .BorderBottom(1).BorderColor(P.Border)
                                     .BorderRight(1).BorderColor(P.Border)
                                     .AlignCenter().AlignMiddle()
                                     .Padding(2);

                                if (comIndex > 1) // segunda comisión en adelante
                                    comCell = comCell
                                        .BorderTop(1.5f).BorderColor(P.BorderSep);

                                comCell
                                     .Element(b => b
                                         .Background(P.Blue)
                                         .PaddingHorizontal(5).PaddingVertical(3)
                                         .AlignCenter())
                                     .Text(gCom.Key)
                                     .FontSize(7f).Bold().FontColor(P.White);

                                // ── CELDAS DE DÍAS ────────────────────────
                                foreach (var dia in dias)
                                {
                                    var clases = gCom
                                        .Where(h => NormalizarDia(h.Dia)
                                            .Equals(dia, StringComparison.OrdinalIgnoreCase))
                                        .OrderBy(h => h.HoraInicio)
                                        .ToList();

                                    // Borde superior separador en comisiones no-primeras
                                    var dayCell = table.Cell()
                                         .BorderBottom(1).BorderColor(P.Border)
                                         .BorderRight(1).BorderColor(P.Border)
                                         .Background(bgRow)
                                         .Padding(3);

                                    if (comIndex > 1)
                                        dayCell = dayCell
                                            .BorderTop(1.5f).BorderColor(P.BorderSep);

                                    dayCell.Column(dayCol =>
                                    {
                                        dayCol.Spacing(3);

                                        foreach (var cl in clases)
                                        {
                                            // ── TARJETA HORIZONTAL ────────
                                            dayCol.Item()
                                                  .Border(0.5f).BorderColor(P.Border)
                                                  .Background(P.White)
                                                  .Row(cardRow =>
                                                  {
                                                      // IZQUIERDA: hora (cb-time)
                                                      cardRow.ConstantItem(34)
                                                             .Background(P.Blue)
                                                             .AlignCenter().AlignMiddle()
                                                             .Padding(2)
                                                             .Column(timeCol =>
                                                             {
                                                                 timeCol.Spacing(0);

                                                                 timeCol.Item()
                                                                        .AlignCenter()
                                                                        .Text(cl.HoraInicio
                                                                            .ToString(@"hh\:mm"))
                                                                        .FontSize(6f).Bold()
                                                                        .FontColor(P.White);

                                                                 // cb-line
                                                                 timeCol.Item()
                                                                        .PaddingVertical(1)
                                                                        .PaddingHorizontal(5)
                                                                        .LineHorizontal(0.5f)
                                                                        .LineColor("#7d96b1");

                                                                 timeCol.Item()
                                                                        .AlignCenter()
                                                                        .Text(cl.HoraFin
                                                                            .ToString(@"hh\:mm"))
                                                                        .FontSize(6f).Bold()
                                                                        .FontColor(P.White);
                                                             });

                                                      // DERECHA: info (cb-body)
                                                      cardRow.RelativeItem()
                                                             .BorderLeft(2f).BorderColor(P.Teal)
                                                             .PaddingHorizontal(4).PaddingVertical(2)
                                                             .Column(body =>
                                                             {
                                                                 body.Spacing(1);

                                                                 // código (cb-code)
                                                                 if (!string.IsNullOrWhiteSpace(cl.Codigo))
                                                                 {
                                                                     body.Item()
                                                                         .Text(cl.Codigo.ToUpper())
                                                                         .FontSize(5f).Bold()
                                                                         .FontColor(P.Blue);
                                                                 }

                                                                 // nombre materia (cb-subject)
                                                                 string materia = (cl.Materia
                                                                     ?? "Materia no informada").ToUpper();
                                                                 body.Item()
                                                                     .Text(materia)
                                                                     .FontSize(6.5f).Bold()
                                                                     .FontColor(P.Text);

                                                                 // aula
                                                                 string aula = string.IsNullOrWhiteSpace(cl.Aula)
                                                                     ? "Aula: A confirmar"
                                                                     : $"Aula: {cl.Aula.ToUpper()}";
                                                                 body.Item()
                                                                     .Text(aula)
                                                                     .FontSize(5.5f).SemiBold()
                                                                     .FontColor(P.TextSoft);

                                                                 // profesor
                                                                 string prof = string.IsNullOrWhiteSpace(cl.Profesor)
                                                                     ? "Profesor sin asignar"
                                                                     : cl.Profesor;
                                                                 body.Item()
                                                                     .Text(prof)
                                                                     .FontSize(5.5f).Italic()
                                                                     .FontColor(P.TextSoft);
                                                             });
                                                  }); // fin cardRow
                                        } // fin foreach clases
                                    }); // fin dayCol
                                } // fin foreach dia
                            } // fin foreach gCom
                        } // fin foreach gAnio
                    }); // fin Table
                }); // fin Page
            }); // fin Document
        }

        // Quita tildes para comparar días con o sin acento
        private static string NormalizarDia(string dia)
        {
            if (string.IsNullOrWhiteSpace(dia)) return dia ?? "";
            return dia
                .Replace("é", "e").Replace("É", "E")
                .Replace("á", "a").Replace("Á", "A")
                .Replace("ó", "o").Replace("Ó", "O")
                .Replace("í", "i").Replace("Í", "I")
                .Replace("ú", "u").Replace("Ú", "U");
        }

        // ================================================================
        // HELPERS — Certificado Regular
        // ================================================================

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

        // ================================================================
        // HELPERS — Footer y rutas
        // ================================================================

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
