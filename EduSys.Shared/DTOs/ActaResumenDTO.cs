namespace EduSys.Shared.DTOs
{
    public class ActaResumenDTO
    {
        public int IdActa { get; set; }
        public string TipoActa { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Libro { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public decimal? NotaAlumno { get; set; }
        public string EstadoAlumno { get; set; } = string.Empty;
        public int IdReferencia { get; set; }

        // 🚀 NUEVA PROPIEDAD
        public string DocenteTitular { get; set; } = string.Empty;
    }
}