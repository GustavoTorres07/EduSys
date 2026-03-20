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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Alias para evitar conflictos
using Color = iText.Kernel.Colors.Color;

namespace EduSys.Web.Services
{
    public class HorarioPdfService
    {
        private readonly ILogger<HorarioPdfService> _logger;

        // ==========================================
        // COLORES INSTITUCIONALES (Mismos que la UI)
        // ==========================================
        private static readonly Color AzulOscuro = new DeviceRgb(30, 58, 88);       // --edusys-primary
        private static readonly Color AzulClaro = new DeviceRgb(69, 105, 144);      // --edusys-primary-light
        private static readonly Color NaranjaAcento = new DeviceRgb(217, 119, 6);   // --edusys-accent-dark
        private static readonly Color GrisFondo = new DeviceRgb(241, 245, 249);     // Gris muy suave para celdas
        private static readonly Color GrisTexto = new DeviceRgb(51, 65, 85);        // Gris oscuro para horas

        public HorarioPdfService(ILogger<HorarioPdfService> logger)
        {
            _logger = logger;
        }

        public byte[] GenerarHorarioPdf(CarreraDTO carrera, string periodo, List<HorarioVisualizacionDTO> horarios, byte[] fontBytes)
        {
            try
            {
                using var stream = new MemoryStream();
                using var writer = new PdfWriter(stream);
                using var pdf = new PdfDocument(writer);

                // 1. Configuración A4 Horizontal (Para que entren bien los días)
                var pageSize = PageSize.A4.Rotate();
                pdf.SetDefaultPageSize(pageSize);

                var document = new Document(pdf);
                document.SetMargins(20, 20, 20, 20);

                // 2. Fuente Incrustada
                PdfFont font = PdfFontFactory.CreateFont(fontBytes, PdfEncodings.IDENTITY_H);
                document.SetFont(font);

                // 3. Encabezados del Documento
                document.Add(new Paragraph("CRONOGRAMA DE HORARIOS")
                    .SetFontColor(AzulClaro).SetFontSize(10).SetBold().SetMarginBottom(0));

                document.Add(new Paragraph(carrera.Nombre.ToUpper())
                    .SetFontColor(AzulOscuro).SetFontSize(16).SetBold().SetMarginBottom(5));

                // Determinar la Sede (Tomamos la primera que aparezca, o dejamos genérico si no hay)
                string sedeNombre = horarios.FirstOrDefault()?.Sede ?? "SEDE A CONFIRMAR";

                var bannerTabla = new Table(1).UseAllAvailableWidth();
                bannerTabla.AddCell(new Cell().Add(new Paragraph($"{periodo.ToUpper()} — {sedeNombre.ToUpper()}"))
                    .SetBackgroundColor(AzulOscuro).SetFontColor(ColorConstants.WHITE)
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(10).SetBold()
                    .SetBorder(Border.NO_BORDER).SetPadding(5));
                document.Add(bannerTabla);

                // 4. Determinar los Días a mostrar en las columnas
                var diasSemanales = new List<string> { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
                if (horarios.Any(h => h.Dia.Equals("Sábado", StringComparison.OrdinalIgnoreCase)))
                {
                    diasSemanales.Add("Sábado");
                }

                // 5. Configurar anchos de la Tabla Matricial
                int totalColumnas = 2 + diasSemanales.Count; // Año, Comisión + Días
                float[] anchosColumnas = new float[totalColumnas];
                anchosColumnas[0] = 5f;  // Columna Año (Estrecha)
                anchosColumnas[1] = 8f;  // Columna Comisión

                float anchoDia = 87f / diasSemanales.Count; // Repartir el resto entre los días
                for (int i = 2; i < totalColumnas; i++)
                {
                    anchosColumnas[i] = anchoDia;
                }

                var table = new Table(UnitValue.CreatePercentArray(anchosColumnas)).UseAllAvailableWidth();
                table.SetMarginTop(10);
                table.SetFixedLayout();

                // --- CABECERAS DE LA TABLA ---
                // Esquina blanca superior izquierda (Ocupa Año y Comisión)
                var esquinaVacia = new Cell(1, 2)
                    .SetBorderTop(Border.NO_BORDER)
                    .SetBorderLeft(Border.NO_BORDER)
                    .SetBorderRight(new SolidBorder(AzulOscuro, 1f))
                    .SetBorderBottom(new SolidBorder(AzulOscuro, 1f));
                table.AddHeaderCell(esquinaVacia);

                // Cabeceras de los Días
                foreach (var dia in diasSemanales)
                {
                    table.AddHeaderCell(new Cell().Add(new Paragraph(dia.ToUpper()))
                        .SetBackgroundColor(NaranjaAcento).SetFontColor(ColorConstants.WHITE)
                        .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetFontSize(10).SetBold().SetBorder(new SolidBorder(AzulOscuro, 1f)));
                }

                // --- CUERPO DE LA TABLA ---
                var gruposAnio = horarios.GroupBy(h => h.AnioCursada).OrderBy(g => g.Key).ToList();

                foreach (var gAnio in gruposAnio)
                {
                    var comisiones = gAnio.GroupBy(h => h.ComisionCodigo).OrderBy(g => g.Key).ToList();
                    int rowSpanAnio = comisiones.Count;
                    bool esPrimeraComisionDelAnio = true;

                    foreach (var gCom in comisiones)
                    {
                        // 1. Celda del Año (Cruza hacia abajo usando RowSpan)
                        if (esPrimeraComisionDelAnio)
                        {
                            // Formato con saltos de línea para que quede vertical sin rotar fuentes (más seguro en iText)
                            var cellAnio = new Cell(rowSpanAnio, 1)
                                .Add(new Paragraph($"{gAnio.Key}º\nAÑO"))
                                .SetBackgroundColor(new DeviceRgb(255, 251, 235)) // Amarillo muy pálido
                                .SetFontColor(NaranjaAcento)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBold().SetFontSize(12)
                                .SetBorder(new SolidBorder(AzulOscuro, 1f));

                            table.AddCell(cellAnio);
                            esPrimeraComisionDelAnio = false;
                        }

                        // 2. Celda de la Comisión
                        table.AddCell(new Cell()
                            .Add(new Paragraph(gCom.Key))
                            .SetBackgroundColor(GrisFondo).SetFontColor(AzulOscuro)
                            .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBold().SetFontSize(10)
                            .SetBorder(new SolidBorder(AzulOscuro, 1f)));

                        // 3. Celdas de los Días (Las Materias)
                        foreach (var dia in diasSemanales)
                        {
                            var clasesEnEsteDia = gCom
                                .Where(h => h.Dia.Equals(dia, StringComparison.OrdinalIgnoreCase))
                                .OrderBy(h => h.HoraInicio)
                                .ToList();

                            var cellDia = new Cell()
                                .SetPadding(5)
                                .SetVerticalAlignment(VerticalAlignment.TOP)
                                .SetBorder(new SolidBorder(AzulOscuro, 1f));

                            if (clasesEnEsteDia.Any())
                            {
                                foreach (var clase in clasesEnEsteDia)
                                {
                                    // Bloque de la clase
                                    var pTime = new Paragraph($"{clase.HoraInicio:hh\\:mm} a {clase.HoraFin:hh\\:mm} hs")
                                        .SetFontSize(8).SetFontColor(GrisTexto).SetBold().SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1);

                                    var pSubj = new Paragraph(clase.Materia.ToUpper())
                                        .SetFontSize(9).SetFontColor(AzulOscuro).SetBold().SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(1);

                                    var pRoom = new Paragraph($"AULA {(string.IsNullOrWhiteSpace(clase.Aula) ? "A CONFIRMAR" : clase.Aula.ToUpper())}")
                                        .SetFontSize(7).SetFontColor(NaranjaAcento).SetBold().SetTextAlignment(TextAlignment.CENTER);

                                    cellDia.Add(pTime).Add(pSubj).Add(pRoom);

                                    // Línea separadora si hay más de una materia el mismo día
                                    if (clase != clasesEnEsteDia.Last())
                                    {
                                        cellDia.Add(new Paragraph(new string('-', 15))
                                            .SetFontColor(ColorConstants.LIGHT_GRAY).SetTextAlignment(TextAlignment.CENTER)
                                            .SetMarginTop(2).SetMarginBottom(2));
                                    }
                                }
                            }

                            table.AddCell(cellDia);
                        }
                    }
                }

                document.Add(table);
                document.Close();
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción crítica al generar el documento PDF del horario.");
                throw new ApplicationException("No se pudo generar el documento PDF. Revise la consola para más detalles.");
            }
        }
    }
}