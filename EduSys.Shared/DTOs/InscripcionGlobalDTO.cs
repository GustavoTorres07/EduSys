namespace EduSys.Shared.DTOs
{
    public class InscripcionGlobalDTO
    {
        public int IdInscripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string AlumnoNombre { get; set; } = string.Empty;
        public string AlumnoLegajo { get; set; } = string.Empty;
        public string AlumnoDni { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public string Comision { get; set; } = string.Empty; 
        public string Estado { get; set; } = string.Empty;
    }
}
