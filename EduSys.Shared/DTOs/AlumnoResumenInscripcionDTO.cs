namespace EduSys.Shared.DTOs
{
    public class AlumnoResumenInscripcionDTO
    {
        public int IdAlumno { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public int CantidadMaterias { get; set; } 
    }
}
