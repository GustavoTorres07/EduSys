namespace EduSys.Shared.DTOs
{
    public class AlumnoActaFinalDTO
    {
        public int IdInscripcion { get; set; }
        public int IdAlumno { get; set; }
        public string Legajo { get; set; } = string.Empty;
        public string AlumnoNombre { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Condicion { get; set; } = string.Empty; 
        public decimal? Nota { get; set; }
        public string EstadoInscripcion { get; set; } = string.Empty; 
    }
}
