namespace EduSys.Shared.DTOs
{
    public class ComisionDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int IdPlanMateria { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public int IdPeriodo { get; set; }
        public string PeriodoNombre { get; set; } = string.Empty;
        public int IdSede { get; set; }
        public string SedeNombre { get; set; } = string.Empty;
        public int CupoMaximo { get; set; }
        public string Turno { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int AnioCursada { get; set; }
        public bool Activo { get; set; }
        public bool EsMateriaLibre { get; set; } = false;
        public string Horarios { get; set; } = string.Empty; 
        public bool CumpleCorrelativas { get; set; } = true;
        public string? MensajeError { get; set; }
        public string Profesor { get; set; } = "Profesor aún no asignado";
        public int CupoDisponible { get; set; }
        public bool YaInscripto { get; set; }
        public string Materia { get; set; } = string.Empty; 
        public string Horario { get; set; } = string.Empty;
        public int CupoActual { get; set; } 
        public List<DocenteComisionListadoDTO> Docentes { get; set; } = new();
    }
}