namespace EduSys.Shared.DTOs
{
    public class HorarioRequestDTO
    {
        public string AlumnoNombre { get; set; } = "";
        public string AlumnoLegajo { get; set; } = "";
        public string AlumnoDni { get; set; } = "";
        public string CarreraNombre { get; set; } = "";
        public string SedeNombre { get; set; } = "";
        public string Periodo { get; set; } = "";
        public List<HorarioVisualizacionDTO> Horarios { get; set; } = new();
    }
}
