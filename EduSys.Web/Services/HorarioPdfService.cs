using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using EduSys.Shared.DTOs;
using System.IO;
using System.Collections.Generic;
using System.Linq;

// Alias para evitar conflictos
using Color = iText.Kernel.Colors.Color;

namespace EduSys.Web.Services
{
    public class HorarioPdfService
    {
        // ==========================================
        // COLORES
        // ==========================================
        private static readonly Color AzulOscuro = new DeviceRgb(63, 85, 118);
        private static readonly Color AzulClaro = new DeviceRgb(240, 245, 250);
        private static readonly Color TealFondo = new DeviceRgb(224, 242, 241);
        private static readonly Color GrisTexto = new DeviceRgb(50, 50, 50);

        public byte[] GenerarHorarioPdf(CarreraDTO carrera, string periodo, List<HorarioVisualizacionDTO> horarios, byte[] fontBytes)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);

            // 1. Configuración A4 Horizontal
            var pageSize = PageSize.A4.Rotate();
            pdf.SetDefaultPageSize(pageSize);

            var document = new Document(pdf);
            document.SetMargins(20, 15, 15, 15);

            // 2. Fuente Incrustada
            PdfFont font = PdfFontFactory.CreateFont(fontBytes, PdfEncodings.IDENTITY_H);
            document.SetFont(font);

            // 3. Encabezados
            document.Add(new Paragraph($"CARRERA: {carrera.Nombre.ToUpper()}")
                .SetFontColor(AzulOscuro).SetFontSize(14).SetBold());

            var tableHeader = new Table(1).UseAllAvailableWidth();
            tableHeader.AddCell(new Cell().Add(new Paragraph($"HORARIO DE CLASES - {periodo.ToUpper()}"))
                .SetBackgroundColor(AzulOscuro).SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(11).SetBold()
                .SetBorder(Border.NO_BORDER).SetPadding(5));
            document.Add(tableHeader);

            // 4. Procesar Datos
            var diasOrden = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            var filasParaTabla = new List<(string Dia, string Curso, List<HorarioVisualizacionDTO> Materias, bool EsPrimerCursoDelDia, int TotalCursosDia)>();

            foreach (var dia in diasOrden)
            {
                var clasesDelDia = horarios.Where(h => h.Dia.Equals(dia, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!clasesDelDia.Any()) continue;

                var gruposCurso = clasesDelDia.GroupBy(x => new { x.AnioCursada, x.ComisionCodigo })
                                              .OrderBy(g => g.Key.AnioCursada).ThenBy(g => g.Key.ComisionCodigo).ToList();

                for (int i = 0; i < gruposCurso.Count; i++)
                {
                    filasParaTabla.Add((
                        Dia: dia,
                        Curso: gruposCurso[i].Key.ComisionCodigo,
                        Materias: gruposCurso[i].OrderBy(x => x.HoraInicio).ToList(),
                        EsPrimerCursoDelDia: i == 0,
                        TotalCursosDia: gruposCurso.Count
                    ));
                }
            }

            // 5. Cálculo Altura
            float availablePoints = 450f;
            int totalRows = filasParaTabla.Count > 0 ? filasParaTabla.Count : 1;
            float rowHeight = availablePoints / totalRows;

            if (rowHeight < 35f) rowHeight = 35f;
            if (rowHeight > 80f) rowHeight = 80f;

            // 6. Tabla
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 8, 12, 80 })).UseAllAvailableWidth();
            table.SetMarginTop(10);
            table.SetFixedLayout();

            table.AddHeaderCell(CrearHeaderCell("DÍA"));
            table.AddHeaderCell(CrearHeaderCell("CURSO"));
            table.AddHeaderCell(CrearHeaderCell("ASIGNATURAS Y HORARIOS"));

            foreach (var fila in filasParaTabla)
            {
                // Columna DÍA
                if (fila.EsPrimerCursoDelDia)
                {
                    table.AddCell(new Cell(fila.TotalCursosDia, 1)
                        .Add(new Paragraph(fila.Dia.Substring(0, 3).ToUpper()))
                        .SetBackgroundColor(AzulOscuro).SetFontColor(ColorConstants.WHITE)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE).SetHorizontalAlignment(HorizontalAlignment.CENTER)
                        .SetBold().SetFontSize(9));
                }

                // Columna CURSO
                table.AddCell(new Cell()
                    .Add(new Paragraph(fila.Curso))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE).SetHorizontalAlignment(HorizontalAlignment.CENTER)
                    .SetFontColor(AzulOscuro).SetBold().SetFontSize(10)
                    .SetHeight(rowHeight));

                // Columna MATERIAS
                int numMaterias = fila.Materias.Count > 0 ? fila.Materias.Count : 1;
                var nestedTable = new Table(numMaterias).UseAllAvailableWidth();

                foreach (var m in fila.Materias)
                {
                    // --- AQUÍ ESTABA EL ERROR (cellM vs cellMateria) ---
                    var cellMateria = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(2)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE);

                    // Borde derecho separador
                    if (m != fila.Materias.Last())
                    {
                        cellMateria.SetBorderRight(new SolidBorder(AzulOscuro, 0.5f)); // CORREGIDO: Ahora usa cellMateria
                    }

                    // Contenido
                    cellMateria.Add(new Paragraph($"{m.HoraInicio:hh\\:mm} - {m.HoraFin:hh\\:mm}")
                        .SetBackgroundColor(TealFondo).SetFontColor(AzulOscuro).SetFontSize(8)
                        .SetBold().SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(2));

                    cellMateria.Add(new Paragraph(m.Materia.ToUpper())
                        .SetFontColor(GrisTexto).SetFontSize(8).SetBold()
                        .SetTextAlignment(TextAlignment.CENTER).SetMultipliedLeading(1.0f));

                    cellMateria.Add(new Paragraph($"({m.Aula})")
                        .SetFontColor(ColorConstants.GRAY).SetFontSize(7).SetItalic()
                        .SetTextAlignment(TextAlignment.CENTER).SetMarginTop(2));

                    nestedTable.AddCell(cellMateria);
                }

                if (fila.Materias.Count == 0) nestedTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));

                table.AddCell(new Cell().Add(nestedTable).SetPadding(0).SetVerticalAlignment(VerticalAlignment.MIDDLE));
            }

            document.Add(table);
            document.Close();
            return stream.ToArray();
        }

        private Cell CrearHeaderCell(string texto)
        {
            return new Cell().Add(new Paragraph(texto))
                .SetBackgroundColor(AzulOscuro).SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFontSize(9).SetBold();
        }
    }
}