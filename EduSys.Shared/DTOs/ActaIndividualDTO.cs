namespace EduSys.Shared.DTOs
{
    public class ActaIndividualDTO
    {
        public int IdActa { get; set; }
        public string NumeroActa { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string TipoActa { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public decimal? Nota { get; set; }
        public string EstadoAcademico { get; set; } = string.Empty;

        // Datos del Alumno
        public string AlumnoNombre { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;

        // Datos de la Materia/Carrera
        public string MateriaNombre { get; set; } = string.Empty;
        public string CarreraNombre { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
        public string DocenteFirma { get; set; } = "A designar";
    }
}