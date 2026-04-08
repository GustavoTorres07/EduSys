namespace EduSys.Shared.DTOs
{
    public class HorarioVisualizacionDTO
    {
        public int Id { get; set; }
        public int IdComision { get; set; }
        public string Materia { get; set; } = null!;
        public int AnioCursada { get; set; }
        public string Curso { get; set; } = null!;
        public string CarreraNombre { get; set; } = null!;
        public string ComisionCodigo { get; set; } = null!;
        public string Dia { get; set; } = null!;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Aula { get; set; } = null!;
        public string Sede { get; set; } = null!;
        public string? Turno { get; set; }
        public string? Codigo { get; set; } 
        public string? Profesor { get; set; }
        public string? PeriodoNombre { get; set; } 
    }
}
