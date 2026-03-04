using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
using Microsoft.AspNetCore.Hosting; // 👈 IMPORTANTE PARA BUSCAR ARCHIVOS
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System;

namespace EduSys.Api.Services
{
    public class PdfReportService : IPdfReportService
    {
        private readonly IWebHostEnvironment _env; // 👈 NUEVA VARIABLE

        // 👈 NUEVO CONSTRUCTOR
        public PdfReportService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] GenerarConstanciaInscripcion(ConstanciaInscripcionDTO data)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    // --- HEADER ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(data.InstitucionNombre).FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("Departamento de Alumnos").FontSize(10).FontColor(Colors.Grey.Darken1);
                            col.Item().Text($"Sede: {data.Sede}").FontSize(10).Bold();
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text("CONSTANCIA DE INSCRIPCIÓN").FontSize(14).Bold().Underline();
                            col.Item().AlignRight().Text($"Ciclo: {data.PeriodoAcademico}").FontSize(12).SemiBold();
                            col.Item().AlignRight().Text($"Fecha: {data.FechaEmision:dd/MM/yyyy HH:mm} hs").FontSize(9).Italic();
                        });
                    });

                    // --- BODY ---
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t => { t.Span("Alumno: ").Bold(); t.Span(data.AlumnoNombre); });
                                c.Item().Text(t => { t.Span("Documento: ").Bold(); t.Span(data.Dni); });
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t => { t.Span("Legajo: ").Bold(); t.Span(data.Legajo); });
                                c.Item().Text(t => { t.Span("Carrera: ").Bold(); t.Span(data.Carrera); });
                            });
                        });

                        col.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().Text("Por medio de la presente se deja constancia que el alumno mencionado se encuentra inscripto en las siguientes asignaturas:").FontSize(11);
                        col.Item().Height(10);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#");
                                header.Cell().Element(CellStyle).Text("Materia");
                                header.Cell().Element(CellStyle).Text("Año");
                                header.Cell().Element(CellStyle).Text("Comisión");
                                header.Cell().Element(CellStyle).Text("Horarios");

                                static IContainer CellStyle(IContainer container) => container.Background(Colors.Blue.Medium).Border(1).BorderColor(Colors.Blue.Medium).Padding(5).AlignMiddle();
                            });

                            for (int i = 0; i < data.Materias.Count; i++)
                            {
                                var item = data.Materias[i];
                                var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                                table.Cell().Element(c => CellData(c, bgColor)).Text($"{i + 1}");
                                table.Cell().Element(c => CellData(c, bgColor)).Text(item.Materia).SemiBold();
                                table.Cell().Element(c => CellData(c, bgColor)).Text($"{item.AnioCursada}°");
                                table.Cell().Element(c => CellData(c, bgColor)).Text(item.Comision);
                                table.Cell().Element(c => CellData(c, bgColor)).Text(string.IsNullOrEmpty(item.Horarios) ? "A confirmar" : item.Horarios).FontSize(9);
                            }

                            static IContainer CellData(IContainer container, string color) => container.Background(color).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignMiddle();
                        });

                        col.Item().PaddingTop(20).Text("Se extiende la presente constancia a pedido del interesado.").FontSize(10).Italic();
                    });

                    // --- FOOTER ---
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Black);
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text("Documento generado automáticamente por EduSys.").FontSize(8).FontColor(Colors.Grey.Medium);
                            row.RelativeItem().AlignRight().Text(x => { x.Span("Página "); x.CurrentPageNumber(); x.Span(" de "); x.TotalPages(); });
                        });
                        col.Item().AlignRight().PaddingTop(5).Text($"Hash: {Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}").FontSize(6).FontColor(Colors.Grey.Lighten1);
                    });
                });
            });

            return pdf.GeneratePdf();
        }

        public byte[] GenerarConstanciaInscripcionFinal(ConstanciaFinalDTO datos)
        {
            var AzulOficial = "#3F5C7A";
            var GrisClaro = "#F4F6F8";
            var GrisBorde = "#bfc3c7";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor(Colors.Black));

                    // --- HEADER ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("INSTITUTO DE EDUCACIÓN SUPERIOR").FontSize(12).Bold().FontColor(AzulOficial);
                            col.Item().Text("DEPARTAMENTO DE ALUMNOS").FontSize(8).FontColor(Colors.Grey.Darken2);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text("CONSTANCIA DE INSCRIPCIÓN A EXAMEN").FontSize(11).Bold().Underline();
                            col.Item().AlignRight().Text($"Nº Transacción: #{datos.NumeroTransaccion:D6}").FontSize(9);
                            col.Item().AlignRight().Text($"Emisión: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).Italic();
                        });
                    });

                    // --- CONTENT ---
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().PaddingTop(30);

                        col.Item().Text(text =>
                        {
                            text.ParagraphSpacing(10);
                            text.Span("La autoridad que suscribe, deja constancia que el/la alumno/a ");
                            text.Span(datos.AlumnoNombreCompleto.ToUpper()).Bold();
                            text.Span(", Documento Nacional de Identidad N° ");
                            text.Span(datos.AlumnoDNI).Bold();
                            text.Span(" y Legajo N° ");
                            text.Span(datos.AlumnoLegajo).Bold();
                            text.Span(", se encuentra formalmente inscripto/a para rendir el examen final de la asignatura detallada a continuación, en el marco de la carrera ");
                            text.Span(datos.CarreraNombre.ToUpper()).Bold();
                            text.Span(".");
                        });

                        col.Item().PaddingTop(20).PaddingBottom(20).Background(GrisClaro).Border(1).BorderColor(GrisBorde).Padding(15).Column(box =>
                        {
                            box.Spacing(5);
                            box.Item().Row(r => { r.RelativeItem().Text("ASIGNATURA:").FontSize(8).Bold().FontColor(Colors.Grey.Darken2); r.RelativeItem(3).Text(datos.MateriaNombre.ToUpper()).FontSize(10).Bold(); });
                            box.Item().Row(r => { r.RelativeItem().Text("FECHA Y HORA:").FontSize(8).Bold().FontColor(Colors.Grey.Darken2); r.RelativeItem(3).Text($"{datos.FechaExamen:dd/MM/yyyy} - {datos.FechaExamen:HH:mm} hs.").FontSize(10); });
                            box.Item().Row(r => { r.RelativeItem().Text("CONDICIÓN:").FontSize(8).Bold().FontColor(Colors.Grey.Darken2); r.RelativeItem(3).Text(datos.Condicion.ToUpper()).FontSize(10).Bold(); });
                            box.Item().Row(r => { r.RelativeItem().Text("TRIBUNAL:").FontSize(8).Bold().FontColor(Colors.Grey.Darken2); r.RelativeItem(3).Text(datos.Tribunal).FontSize(10); });
                        });

                        col.Item().PaddingTop(10).Text("Se extiende la presente constancia a pedido del interesado/a para ser presentada ante el tribunal examinador y las autoridades que lo requieran.");

                        // --- SELLO Y FIRMA CORREGIDOS ---
                        string rutaImagen = ObtenerRutaSelloLocal();
                        if (System.IO.File.Exists(rutaImagen))
                        {
                            var imagenBytes = System.IO.File.ReadAllBytes(rutaImagen);
                            col.Item().PaddingTop(40).AlignRight().Width(5, Unit.Centimetre).Image(imagenBytes);
                        }

                        col.Item().PaddingTop(5).AlignRight().Column(c =>
                        {
                            c.Item().Text("___________________________").FontColor(Colors.Grey.Lighten1);
                            c.Item().Text("Secretaría Académica").FontSize(9).Bold();
                            c.Item().Text("Firma y Sello").FontSize(8).Italic();
                        });
                    });

                    // --- FOOTER ---
                    page.Footer().Column(f => {
                        f.Item().LineHorizontal(1).LineColor(Colors.Black);
                        f.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text("Documento generado automáticamente por sistema EduSys.").FontSize(7).FontColor(Colors.Grey.Medium);
                            row.RelativeItem().AlignRight().Text(x => { x.Span("Página "); x.CurrentPageNumber(); x.Span(" de "); x.TotalPages(); });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }


        // =========================================================================
        // MÉTODO: ANALÍTICO PROVISORIO (HISTORIA ACADÉMICA)
        // =========================================================================
        // =========================================================================
        // MÉTODO: ANALÍTICO PROVISORIO (COMPACTO - 1 PÁGINA)
        // =========================================================================
        public byte[] GenerarAnaliticoProvisorio(HistoriaAcademicaDTO datos)
        {
            var AzulOficial = "#3F5C7A";
            var GrisClaro = "#F4F6F8";
            var GrisBorde = "#bfc3c7";

            // Calculamos datos resumen basados en tu DTO "Detalle"
            var materiasAprobadas = datos.Detalle.Where(m => m.Nota >= 4 || m.Estado == "Promocionado" || m.Estado == "Aprobado" || m.Estado == "Equivalencia").ToList();
            int totalAprobadas = materiasAprobadas.Count;

            // Usamos las estadísticas que ya vienen en tu DTO o las calculamos
            decimal promedio = datos.PromedioGeneral > 0
                ? datos.PromedioGeneral
                : (totalAprobadas > 0 && materiasAprobadas.Any(m => m.Nota.HasValue)
                    ? materiasAprobadas.Where(m => m.Nota.HasValue).Average(m => m.Nota!.Value)
                    : 0);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    // 1. REDUCIMOS LOS MÁRGENES A 1 CENTÍMETRO PARA GANAR ESPACIO
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);

                    // 2. FUENTE BASE MÁS PEQUEÑA
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial").FontColor(Colors.Black));

                    // --- HEADER ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("INSTITUTO DE EDUCACIÓN SUPERIOR").FontSize(11).Bold().FontColor(AzulOficial);
                            col.Item().Text("DEPARTAMENTO DE ALUMNOS").FontSize(7).FontColor(Colors.Grey.Darken2);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text("ANALÍTICO PROVISORIO").FontSize(11).Bold().Underline();
                            col.Item().AlignRight().Text($"Emisión: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).Italic();
                        });
                    });

                    // --- CONTENT ---
                    page.Content().PaddingVertical(5).Column(col =>
                    {
                        // 1. Datos del Alumno (Caja más compacta)
                        col.Item().PaddingTop(10).Background(GrisClaro).Border(1).BorderColor(GrisBorde).Padding(6).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t => { t.Span("ALUMNO: ").Bold().FontSize(7).FontColor(Colors.Grey.Darken2); t.Span(datos.AlumnoNombre.ToUpper()).FontSize(8).Bold(); });
                                c.Item().Text(t => { t.Span("LEGAJO: ").Bold().FontSize(7).FontColor(Colors.Grey.Darken2); t.Span(datos.Legajo).FontSize(8); });
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t => { t.Span("CARRERA: ").Bold().FontSize(7).FontColor(Colors.Grey.Darken2); t.Span(datos.Carrera.ToUpper()).FontSize(8); });
                                c.Item().Text(t => { t.Span("PLAN: ").Bold().FontSize(7).FontColor(Colors.Grey.Darken2); t.Span(datos.Plan.ToUpper()).FontSize(8); });
                            });
                        });

                        // 2. Resumen de Desempeño
                        col.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text($"Materias Aprobadas: {datos.MateriasAprobadas} de {datos.TotalMateriasPlan} ({datos.PorcentajeAvance:0.00}%)").FontSize(9).Bold().FontColor(AzulOficial);
                            row.RelativeItem().AlignRight().Text($"Promedio General: {promedio:0.00}").FontSize(9).Bold().FontColor(AzulOficial);
                        });

                        col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // 3. Tabla de Materias (Historia Académica) Ultra-Compacta
                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(20); // #
                                columns.RelativeColumn(1.5f); // CÓDIGO (Nuevo)
                                columns.RelativeColumn(4);    // ASIGNATURA
                                columns.RelativeColumn(1.5f); // FECHA
                                columns.RelativeColumn(1.5f); // ESTADO
                                columns.RelativeColumn(1);    // NOTA
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#");
                                header.Cell().Element(CellStyle).Text("CÓDIGO");
                                header.Cell().Element(CellStyle).Text("ASIGNATURA");
                                header.Cell().Element(CellStyle).Text("FECHA");
                                header.Cell().Element(CellStyle).Text("ESTADO");
                                header.Cell().Element(CellStyle).Text("NOTA");

                                IContainer CellStyle(IContainer container) => container.Background(AzulOficial).Border(1).BorderColor(AzulOficial).PaddingVertical(2).PaddingHorizontal(2).AlignCenter().AlignMiddle().DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(7));
                            });

                            var materiasAgrupadas = datos.Detalle.GroupBy(m => m.AnioCursada).OrderBy(g => g.Key);
                            int index = 1;

                            foreach (var grupo in materiasAgrupadas)
                            {
                                // Fila separadora del Año (¡Ojo! Ahora el ColumnSpan es 6 porque tenemos 6 columnas)
                                table.Cell().ColumnSpan(6).Background(Colors.Grey.Lighten3).BorderBottom(1).BorderColor(GrisBorde).PaddingVertical(2).PaddingHorizontal(2).AlignCenter().Text($"ASIGNATURAS DE {grupo.Key}° AÑO").Bold().FontSize(7).FontColor(AzulOficial);

                                foreach (var mat in grupo)
                                {
                                    var bgColor = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                                    // #
                                    table.Cell().Element(c => CellData(c, bgColor)).AlignCenter().Text($"{index}");
                                    
                                    // CÓDIGO (Nuevo)
                                    string codigoStr = string.IsNullOrEmpty(mat.Codigo) ? "-" : mat.Codigo;
                                    table.Cell().Element(c => CellData(c, bgColor)).AlignCenter().Text(codigoStr).FontSize(7);

                                    // ASIGNATURA
                                    table.Cell().Element(c => CellData(c, bgColor)).AlignLeft().Text(mat.Materia).SemiBold().FontSize(7.5f); 

                                    // FECHA
                                    string fechaStr = mat.Fecha.HasValue ? mat.Fecha.Value.ToString("dd/MM/yyyy") : "-";
                                    table.Cell().Element(c => CellData(c, bgColor)).AlignCenter().Text(fechaStr).FontSize(7);
                                    
                                    // ESTADO
                                    string estadoStr = string.IsNullOrEmpty(mat.Estado) ? "Pendiente" : mat.Estado;
                                    table.Cell().Element(c => CellData(c, bgColor)).AlignCenter().Text(estadoStr.ToUpper()).FontSize(6.5f);

                                    // NOTA
                                    string notaStr = mat.Nota.HasValue ? mat.Nota.Value.ToString("0.##") : "-";
                                    if (mat.Nota >= 4)
                                    {
                                        table.Cell().Element(c => CellData(c, bgColor)).AlignCenter().Text(notaStr).Bold().FontColor(Colors.Blue.Darken2).FontSize(7.5f);
                                    }
                                    else
                                    {
                                        table.Cell().Element(c => CellData(c, bgColor)).AlignCenter().Text(notaStr).Bold().FontSize(7.5f);
                                    }

                                    index++;
                                }
                            }

                            IContainer CellData(IContainer container, string color) => container.Background(color).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(1).PaddingHorizontal(2).AlignMiddle();
                        });
                        col.Item().PaddingTop(10).Text("El presente analítico tiene carácter de provisorio y es válido únicamente para trámites internos o consulta del estudiante. No certifica graduación.").FontSize(7).FontColor(Colors.Grey.Darken1).Italic();

                        // --- SELLO Y FIRMA COMPACTADOS ---
                        string rutaImagen = ObtenerRutaSelloLocal();
                        if (System.IO.File.Exists(rutaImagen))
                        {
                            var imagenBytes = System.IO.File.ReadAllBytes(rutaImagen);
                            col.Item().PaddingTop(5).AlignRight().Width(3.5f, Unit.Centimetre).Image(imagenBytes);
                        }
                    });

                    // --- FOOTER ---
                    page.Footer().Column(f => {
                        f.Item().LineHorizontal(1).LineColor(Colors.Black);
                        f.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text("Documento generado automáticamente por sistema EduSys.").FontSize(6).FontColor(Colors.Grey.Medium);
                            row.RelativeItem().AlignRight().Text(x => { x.Span("Página "); x.CurrentPageNumber(); x.Span(" de "); x.TotalPages(); });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
        // 👇 MÉTODO PRIVADO CON LA MISMA LÓGICA INFALIBLE QUE EL CONTROLADOR 👇
        private string ObtenerRutaSelloLocal()
        {
            string ruta = Path.Combine(_env.WebRootPath ?? "wwwroot", "images", "sello_edusys.png");
            if (!System.IO.File.Exists(ruta))
            {
                ruta = Path.Combine(AppContext.BaseDirectory, "wwwroot", "images", "sello_edusys.png");
            }
            return ruta;
        }
    }
}