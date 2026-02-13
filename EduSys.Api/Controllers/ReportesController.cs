using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EduSys.Shared.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using EduSys.Api.Repositories.Interfaces;

// Alias
using Document = QuestPDF.Fluent.Document;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace EduSys.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        // PALETA
        private static readonly string AzulOficial = "#1e3a58";
        private static readonly string GrisHeader = "#f4f6f7";
        private static readonly string GrisBorde = "#bfc3c7";
        private static readonly string GrisDivisorMateria = "#9aa1a7";
        private static readonly string GrisTexto = "#2c3e50";
        private static readonly string VerdeTexto = "#145a32";
        private static readonly string FondoMateria = "#fafafa";
        private static readonly string Blanco = "#ffffff";

        // Colores Constancia
        private static readonly string Azul = "#3F5C7A";
        private static readonly string GrisClaro = "#F4F6F8";

        private const float BORDE = 2.0f;
        private const float BORDE_MATERIA = 2.0f;

        // Repositorios
        private readonly IInscripcionRepository _inscripcionRepo;
        private readonly IHorarioRepository _horarioRepo;
        private readonly IReportesRepository _reporteRepo; // Nuevo
        private readonly IWebHostEnvironment _env;

        public ReportesController(
            IInscripcionRepository inscripcionRepo,
            IHorarioRepository horarioRepo,
            IReportesRepository reporteRepo,
            IWebHostEnvironment env)
        {
            _inscripcionRepo = inscripcionRepo;
            _horarioRepo = horarioRepo;
            _reporteRepo = reporteRepo;
            _env = env;
        }

        // =====================================================
        // HORARIOS (Sin Cambios)
        // =====================================================
        [HttpGet("horarios-alumno-cursando")]
        public async Task<IActionResult> GetHorariosCursando([FromQuery] int idPeriodo, [FromQuery] int idAlumno)
        {
            try
            {
                var horarios = await _horarioRepo.GetHorariosCursandoAsync(idPeriodo, idAlumno);
                return Ok(horarios ?? new List<HorarioVisualizacionDTO>());
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("horario-descargar")]
        public async Task<IActionResult> DescargarHorarioGet([FromQuery] int idPeriodo, [FromQuery] int idCarrera, [FromQuery] int idSede)
        {
            var horarios = await _horarioRepo.GetHorariosByCarreraAndPeriodoAsync(idPeriodo, idCarrera, idSede);
            if (!horarios.Any()) return NotFound();

            foreach (var h in horarios)
            {
                h.Curso ??= h.ComisionCodigo ?? "-";
                h.Materia ??= "Materia no informada";
            }

            var first = horarios.First();
            var request = new HorarioRequestDTO
            {
                CarreraNombre = first.CarreraNombre ?? "Carrera",
                SedeNombre = first.Sede ?? "Sede Central",
                Periodo = "CICLO 2026",
                Horarios = horarios
            };

            QuestPDF.Settings.License = LicenseType.Community;
            var pdf = CrearDocumentoVertical(request).GeneratePdf();
            return File(pdf, "application/pdf", $"Horario_{request.CarreraNombre.Replace(" ", "_")}.pdf");
        }

        // =====================================================
        // CONSTANCIA DE INSCRIPCIÓN (Diseño Nuevo)
        // =====================================================
        [HttpGet("constancia-inscripcion")]
        public async Task<IActionResult> DescargarConstancia([FromQuery] int idAlumno, [FromQuery] int idPeriodo)
        {
            try
            {
                var datos = await _reporteRepo.GetDatosConstanciaAsync(idAlumno, idPeriodo);

                if (datos == null || !datos.Materias.Any())
                    return BadRequest("No se encontraron inscripciones para generar la constancia.");

                QuestPDF.Settings.License = LicenseType.Community;

                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1.2f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial").FontColor(Colors.Black));

                        // Header
                        page.Header().Element(c => HeaderConstancia(c, datos));

                        // Body
                        page.Content().PaddingVertical(5).Element(c => BodyConstancia(c, datos, _env.WebRootPath));

                        // Footer
                        page.Footer().Element(c => FooterConstancia(c));
                    });
                });

                var pdfBytes = pdf.GeneratePdf();
                var nombreArchivo = $"Constancia_{datos.Legajo}_{DateTime.Now:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generando PDF: {ex.Message}");
            }
        }


        [HttpGet("historia-academica")]
        public async Task<IActionResult> GetHistoriaAcademica([FromQuery] int idAlumno)
        {
            try
            {
                var historia = await _reporteRepo.GetHistoriaAcademicaAsync(idAlumno);
                if (historia == null) return NotFound("Alumno no encontrado o sin plan asignado.");
                return Ok(historia);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        // =====================================================
        // 3. CERTIFICADO ALUMNO REGULAR (Endpoint)
        // =====================================================
        [HttpGet("certificado-alumno-regular-descargar")]
        public async Task<IActionResult> DescargarCertificadoRegular([FromQuery] int idAlumno, [FromQuery] int idPeriodo)
        {
            try
            {
                var datos = await _reporteRepo.GetDatosCertificadoRegularAsync(idAlumno, idPeriodo);

                if (datos == null)
                    return BadRequest("El alumno no posee inscripciones activas en este período para emitir el certificado.");

                QuestPDF.Settings.License = LicenseType.Community;

                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor(Colors.Black));

                        // Reuse Header (Ligeramente modificado para el título)
                        page.Header().Element(c => HeaderCertificadoRegular(c, datos));

                        // Body Específico
                        page.Content().PaddingVertical(10).Element(c => BodyCertificadoRegular(c, datos));

                        // Reuse Footer
                        page.Footer().Element(c => FooterConstancia(c));
                    });
                });

                var pdfBytes = pdf.GeneratePdf();
                string nombreArchivo = $"Certificado_Regular_{datos.Legajo}_{DateTime.Now:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generando PDF: {ex.Message}");
            }
        }

        // =====================================================
        // HELPERS VISUALES - ALUMNO REGULAR
        // =====================================================

        void HeaderCertificadoRegular(IContainer container, CertificadoAlumnoRegularDTO data)
        {
            container.Row(row =>
            {
                // Izquierda (Institución)
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(data.InstitucionNombre.ToUpper()).FontSize(12).Bold().FontColor(Azul);
                    col.Item().Text(data.Departamento).FontSize(8).FontColor(Colors.Grey.Darken2);
                });

                // Derecha (Título Documento)
                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Text("CERTIFICADO DE ALUMNO REGULAR").FontSize(11).Bold().Underline();
                    col.Item().AlignRight().Text($"Ciclo Lectivo: {data.PeriodoAcademico}").FontSize(9);
                    col.Item().AlignRight().Text($"Emisión: {data.FechaEmision:dd/MM/yyyy}").FontSize(8).Italic();
                });
            });
        }

        void BodyCertificadoRegular(IContainer container, CertificadoAlumnoRegularDTO data)
        {
            container.Column(col =>
            {
                // Espacio inicial
                col.Item().PaddingTop(30);

                // Texto Principal (Formato Nota)
                col.Item().Text(text =>
                {
                    text.ParagraphSpacing(10);

                    // Párrafo 1
                    text.Span("La autoridad que suscribe, certifica que el/la alumno/a ");
                    text.Span(data.AlumnoNombre.ToUpper()).Bold();
                    text.Span($", Documento Nacional de Identidad N° ");
                    text.Span(data.Dni).Bold();
                    text.Span(" y Legajo N° ");
                    text.Span(data.Legajo).Bold();
                    text.Span(", es alumno/a regular de la carrera ");
                    text.Span(data.Carrera.ToUpper()).Bold();
                    text.Span($", que se dicta en la Sede {data.Sede}, durante el Ciclo Lectivo {data.PeriodoAcademico}.").FontSize(10);
                });

                // Caja de Datos Resumen (Estilo similar a la constancia anterior)
                col.Item().PaddingTop(20).PaddingBottom(20).Background(GrisClaro).Border(1).BorderColor(GrisBorde).Padding(15).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("ESTADO ACADÉMICO:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        c.Item().Text("REGULAR").FontSize(12).Bold().FontColor(VerdeTexto);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("FECHA DE INGRESO / REINSCRIPCIÓN:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        // Aquí podrías poner la fecha real de inscripción si la tuvieras en el DTO, usando hoy por defecto
                        c.Item().Text(data.PeriodoAcademico).FontSize(10).Bold();
                    });
                });

                // Texto de Cierre
                col.Item().PaddingTop(10).Text(text =>
                {
                    text.Span("Se extiende el presente certificado a pedido del interesado/a y para ser presentado ante quien corresponda, en la ciudad de ");
                    text.Span(data.Ciudad).Bold();
                    text.Span(", Provincia de ");
                    text.Span(data.Provincia).Bold();
                    text.Span($", a los {data.FechaEmision.Day} días del mes de {data.FechaEmision.ToString("MMMM")} del año {data.FechaEmision.Year}.").FontSize(10);
                });

                // Firma / Sello (Reutilizando el helper de imagen)
                string rutaImagen = ObtenerRutaSello();
                if (System.IO.File.Exists(rutaImagen))
                {
                    var imagenBytes = System.IO.File.ReadAllBytes(rutaImagen);
                    col.Item().PaddingTop(40).AlignRight().Width(5, Unit.Centimetre).Image(imagenBytes);
                }

                // Línea de firma si no hay imagen o debajo de ella
                col.Item().PaddingTop(5).AlignRight().Column(c =>
                {
                    c.Item().Text(data.RectorNombre).FontSize(9).Bold();
                    c.Item().Text(data.RectorCargo).FontSize(8).Italic();
                });
            });
        }

        // ---------------- HELPERS VISUALES CONSTANCIA ----------------

        void HeaderConstancia(IContainer container, ConstanciaInscripcionDTO data)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(data.InstitucionNombre.ToUpper()).FontSize(12).Bold().FontColor(Azul);
                    col.Item().Text("DEPARTAMENTO DE ALUMNOS").FontSize(8).FontColor(Colors.Grey.Darken2);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Text("CONSTANCIA DE INSCRIPCIÓN").FontSize(11).Bold().Underline();
                    col.Item().AlignRight().Text($"Ciclo Lectivo: {data.PeriodoAcademico}").FontSize(9);
                    col.Item().AlignRight().Text($"Emisión: {data.FechaEmision:dd/MM/yyyy HH:mm} hs").FontSize(7).Italic();
                });
            });
        }

        void BodyConstancia(IContainer container, ConstanciaInscripcionDTO data, string webRootPath)
        {
            container.Column(col =>
            {
                col.Item().PaddingTop(8).Text("Por medio de la presente se deja constancia que:").FontSize(9);

                // DATOS ALUMNO
                col.Item().PaddingVertical(6).Background(GrisClaro).Border(1).BorderColor(GrisBorde).Padding(8).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("ALUMNO: ").Bold().FontSize(8); t.Span(data.AlumnoNombre.ToUpper()).FontSize(8); });
                        c.Item().Text(t => { t.Span("DOCUMENTO: ").Bold().FontSize(8); t.Span(data.Dni).FontSize(8); });
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("LEGAJO: ").Bold().FontSize(8); t.Span(data.Legajo).FontSize(8); });
                        c.Item().Text(t => { t.Span("CARRERA: ").Bold().FontSize(8); t.Span(data.Carrera).FontSize(8); });
                        c.Item().Text(t => { t.Span("SEDE: ").Bold().FontSize(8); t.Span(data.Sede).FontSize(8); });
                    });
                });

                col.Item().PaddingTop(6).Text("Se encuentra formalmente inscripto/a en las siguientes asignaturas:").FontSize(9);

                // TABLA MATERIAS
                col.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);  // #
                        columns.RelativeColumn(2);   // Código
                        columns.RelativeColumn(4);   // Materia
                        columns.RelativeColumn(2);   // Comisión
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(EstiloHeaderTabla).Text("#");
                        header.Cell().Element(EstiloHeaderTabla).Text("CÓDIGO");
                        header.Cell().Element(EstiloHeaderTabla).Text("MATERIA");
                        header.Cell().Element(EstiloHeaderTabla).Text("COMISIÓN");
                    });

                    // Agrupado por Año
                    var materiasPorAnio = data.Materias.GroupBy(m => m.AnioCursada).OrderBy(g => g.Key);
                    int numeroGlobal = 1;

                    foreach (var grupo in materiasPorAnio)
                    {
                        // Separador Año
                        table.Cell().ColumnSpan(4).Element(EstiloFilaAnio).Text($"{grupo.Key}° AÑO");

                        foreach (var item in grupo)
                        {
                            var bg = numeroGlobal % 2 == 0 ? GrisClaro : Blanco;
                            table.Cell().Element(c => EstiloCeldaTabla(c, bg)).Text($"{numeroGlobal}");
                            table.Cell().Element(c => EstiloCeldaTabla(c, bg)).Text(item.CodigoMateria ?? "-");
                            table.Cell().Element(c => EstiloCeldaTabla(c, bg)).Text(item.Materia).Bold();
                            table.Cell().Element(c => EstiloCeldaTabla(c, bg)).Text(item.Comision);
                            numeroGlobal++;
                        }
                    }
                });

                col.Item().PaddingTop(15).Text("Se extiende la presente constancia a pedido del interesado/a para ser presentada ante quien corresponda.").FontSize(9);

                // SELLO
                string rutaImagen = ObtenerRutaSello();
                if (System.IO.File.Exists(rutaImagen))
                {
                    var imagenBytes = System.IO.File.ReadAllBytes(rutaImagen);
                    col.Item().PaddingTop(10).AlignRight().Width(5, Unit.Centimetre).Image(imagenBytes);
                }
            });
        }

        void FooterConstancia(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(Colors.Black);
                col.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Documento generado automáticamente por sistema EduSys.").FontSize(7).FontColor(Colors.Grey.Medium);
                    row.RelativeItem().AlignRight().Text(x => { x.Span("Página "); x.CurrentPageNumber(); x.Span(" de "); x.TotalPages(); });
                });
            });
        }

        // Estilos Constancia
        static IContainer EstiloHeaderTabla(IContainer container) => container.Background(Azul).Border(1).BorderColor(Azul).Padding(4).AlignMiddle().AlignCenter().DefaultTextStyle(x => x.Bold().FontColor(Colors.White).FontSize(8));
        static IContainer EstiloCeldaTabla(IContainer container, string bg) => container.Background(bg).BorderBottom(1).BorderColor(GrisBorde).Padding(3).AlignMiddle().AlignCenter().DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Black));
        static IContainer EstiloFilaAnio(IContainer container) => container.Background(AzulOficial).Border(1).BorderColor(AzulOficial).Padding(4).AlignMiddle().AlignCenter().DefaultTextStyle(x => x.Bold().FontColor(Colors.White).FontSize(9));

        // ---------------- HELPERS VISUALES HORARIO (Sin cambios) ----------------
        private Document CrearDocumentoVertical(HorarioRequestDTO request)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0.5f, Unit.Centimetre);
                    page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(6.5f).FontColor(GrisTexto));

                    page.Header().PaddingBottom(2).Row(row =>
                    {
                        row.RelativeItem().Background(AzulOficial).PaddingVertical(5).PaddingHorizontal(6).Column(col =>
                        {
                            col.Item().AlignCenter().Text(request.CarreraNombre.ToUpper()).Bold().FontSize(9).FontColor(Blanco);
                            col.Item().PaddingTop(1).Row(r =>
                            {
                                r.RelativeItem().Text($"HORARIOS {request.Periodo}").FontSize(7).FontColor(Blanco);
                                r.RelativeItem().AlignRight().Text(request.SedeNombre?.ToUpper()).FontSize(6.5f).FontColor(Blanco);
                            });
                        });
                    });

                    page.Content().PaddingTop(2).Table(table =>
                    {
                        table.ColumnsDefinition(columns => { columns.ConstantColumn(20); columns.ConstantColumn(36); columns.RelativeColumn(); });
                        table.Header(header => { header.Cell().Element(EstiloHeader).Text("DÍA"); header.Cell().Element(EstiloHeader).Text("CURSO"); header.Cell().Element(EstiloHeader).Text("ASIGNATURAS Y HORARIOS"); });

                        var dias = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
                        foreach (var dia in dias)
                        {
                            var clasesDia = request.Horarios.Where(h => h.Dia.Equals(dia, StringComparison.OrdinalIgnoreCase)).ToList();
                            if (!clasesDia.Any()) continue;
                            var grupos = clasesDia.GroupBy(c => new { c.AnioCursada, Codigo = (c.ComisionCodigo ?? "S/C").Trim().ToUpper() }).OrderBy(g => g.Key.AnioCursada).ThenBy(g => g.Key.Codigo).ToList();
                            table.Cell().RowSpan((uint)grupos.Count).Element(EstiloCeldaDia).Text(dia.Substring(0, 3).ToUpper()).Bold().FontSize(7.5f).FontColor(Blanco);
                            foreach (var grupo in grupos)
                            {
                                table.Cell().Element(EstiloCeldaCurso).Text(grupo.Key.Codigo).Bold().FontSize(7).FontColor(AzulOficial);
                                table.Cell().Element(EstiloCeldaMaterias).Row(rowMaterias =>
                                {
                                    var materias = grupo.OrderBy(c => c.HoraInicio).ToList();
                                    for (int i = 0; i < materias.Count; i++)
                                    {
                                        var clase = materias[i];
                                        rowMaterias.RelativeItem().Element(c => (i < materias.Count - 1) ? c.BorderRight(BORDE_MATERIA).BorderColor(GrisDivisorMateria) : c).Element(EstiloBloqueMateria).Column(colMat =>
                                        {
                                            colMat.Item().AlignCenter().Text($"{clase.HoraInicio:hh\\:mm}-{clase.HoraFin:hh\\:mm}").FontSize(6).Bold().FontColor(VerdeTexto);
                                            colMat.Item().AlignCenter().Text(clase.Materia.ToUpper()).SemiBold();
                                            if (!string.IsNullOrWhiteSpace(clase.Aula)) colMat.Item().AlignCenter().PaddingTop(0.5f).Text($"({clase.Aula})").FontSize(5.2f).Italic().FontColor(Colors.Grey.Darken1);
                                        });
                                    }
                                });
                            }
                        }
                    });
                    page.Footer().PaddingTop(2).Row(row => { row.RelativeItem().Text($"Generado: {DateTime.Now:dd/MM HH:mm}").FontSize(5.5f); row.RelativeItem().AlignRight().Element(c => { var ruta = ObtenerRutaSello(); if (System.IO.File.Exists(ruta)) c.Height(28).Image(ruta); }); });
                });
            });
        }

        static IContainer EstiloHeader(IContainer c) => c.Background(GrisHeader).Border(BORDE).BorderColor(GrisBorde).PaddingVertical(4).PaddingHorizontal(3).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.Bold().FontSize(7.2f).FontColor(AzulOficial));
        static IContainer EstiloCeldaDia(IContainer c) => c.Background(AzulOficial).Border(BORDE).BorderColor(GrisBorde).RotateLeft().AlignCenter().AlignMiddle();
        static IContainer EstiloCeldaCurso(IContainer c) => c.Border(BORDE).BorderColor(GrisBorde).AlignCenter().AlignMiddle();
        static IContainer EstiloCeldaMaterias(IContainer c) => c.Border(BORDE).BorderColor(GrisBorde);
        static IContainer EstiloBloqueMateria(IContainer c) => c.Background(FondoMateria).PaddingVertical(2.5f).PaddingHorizontal(2.5f).AlignCenter().AlignMiddle();

        private string ObtenerRutaSello()
        {
            string ruta = Path.Combine(_env.WebRootPath ?? "wwwroot", "images", "sello_edusys.png");
            if (!System.IO.File.Exists(ruta)) ruta = Path.Combine(AppContext.BaseDirectory, "wwwroot", "images", "sello_edusys.png");
            return ruta;
        }
    }
}