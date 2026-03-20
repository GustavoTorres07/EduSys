namespace EduSys.Shared.DTOs
{
    public class CursadaAlumnoDTO
    {
        public int IdInscripcion { get; set; }
        public string Materia { get; set; } = string.Empty;
        public string Comision { get; set; } = string.Empty;
        public string EstadoCursada { get; set; } = string.Empty; 
        public decimal? Promedio { get; set; }
        public List<ExamenAlumnoDTO> Examenes { get; set; } = new();
    }
}
