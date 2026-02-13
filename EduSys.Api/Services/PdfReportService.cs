using EduSys.Shared.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduSys.Api.Services
{
    public class PdfReportService
    {
        public byte[] GenerarConstanciaInscripcion(ConstanciaInscripcionDTO data)
        {
            QuestPDF.Settings.License = LicenseType.Community; // Licencia gratuita

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
                        // Logo o Nombre Institución (Izquierda)
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(data.InstitucionNombre).FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("Departamento de Alumnos").FontSize(10).FontColor(Colors.Grey.Darken1);
                            col.Item().Text($"Sede: {data.Sede}").FontSize(10).Bold();
                        });

                        // Título del Documento (Derecha)
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
                        // 1. Datos del Alumno (Recuadro gris)
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

                        // 2. Tabla de Materias
                        col.Item().Table(table =>
                        {
                            // Definir columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30); // #
                                columns.RelativeColumn(3);  // Materia
                                columns.RelativeColumn(1);  // Año
                                columns.RelativeColumn(1);  // Comisión
                                columns.RelativeColumn(2);  // Horarios
                            });

                            // Encabezados
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#");
                                header.Cell().Element(CellStyle).Text("Materia");
                                header.Cell().Element(CellStyle).Text("Año");
                                header.Cell().Element(CellStyle).Text("Comisión");
                                header.Cell().Element(CellStyle).Text("Horarios");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.Background(Colors.Blue.Medium).Border(1).BorderColor(Colors.Blue.Medium).Padding(5).AlignMiddle();
                                }
                            });

                            // Filas
                            for (int i = 0; i < data.Materias.Count; i++)
                            {
                                var item = data.Materias[i];
                                var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5; // Zebra striping

                                table.Cell().Element(c => CellData(c, bgColor)).Text($"{i + 1}");
                                table.Cell().Element(c => CellData(c, bgColor)).Text(item.Materia).SemiBold();
                                table.Cell().Element(c => CellData(c, bgColor)).Text($"{item.AnioCursada}°");
                                table.Cell().Element(c => CellData(c, bgColor)).Text(item.Comision);
                                table.Cell().Element(c => CellData(c, bgColor)).Text(string.IsNullOrEmpty(item.Horarios) ? "A confirmar" : item.Horarios).FontSize(9);
                            }

                            static IContainer CellData(IContainer container, string color)
                            {
                                return container.Background(color).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignMiddle();
                            }
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
                            row.RelativeItem().AlignRight().Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                                x.Span(" de ");
                                x.TotalPages();
                            });
                        });

                        // Código QR o Validación (Simulado)
                        col.Item().AlignRight().PaddingTop(5).Text($"Hash: {Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}").FontSize(6).FontColor(Colors.Grey.Lighten1);
                    });
                });
            });

            return pdf.GeneratePdf();
        }
    }
}