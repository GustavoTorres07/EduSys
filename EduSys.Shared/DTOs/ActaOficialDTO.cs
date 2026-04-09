namespace EduSys.Shared.DTOs
{
    public class ActaOficialDTO
    {
        // --- CABECERA DEL ACTA ---
        public string TipoActa { get; set; } = string.Empty; // "PROMOCIÓN" o "EXAMEN FINAL"
        public string Materia { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string PlanEstudio { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }

        public string Libro { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;

        // Autoridades firmantes
        public string PresidenteMesa { get; set; } = string.Empty;
        public string Vocal1 { get; set; } = string.Empty;
        public string Vocal2 { get; set; } = string.Empty;

        // --- CUERPO DEL ACTA ---
        public List<FilaActaAlumnoDTO> Alumnos { get; set; } = new();
    }

    public class FilaActaAlumnoDTO
    {
        public string Legajo { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;

        public string Condicion { get; set; } = string.Empty; // Promocionado, Aprobado, Aplazado, Ausente
        public decimal? NotaNotaNumerica { get; set; }
        public string NotaLetras { get; set; } = string.Empty; // Ej: "OCHO", "DOS"
    }
}