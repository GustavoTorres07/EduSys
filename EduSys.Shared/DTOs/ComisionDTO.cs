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

        // ✅ Incluye el turno Y los horarios concatenados
        // Ejemplo: "Tarde (Mar 19:20-20:40 / Jue 19:20-20:40)"
        public string Turno { get; set; } = string.Empty;

        // ✅ Nueva propiedad para mostrar el aula
        public string Aula { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public int AnioCursada { get; set; }

        public bool Activo { get; set; }
        public bool EsMateriaLibre { get; set; } = false;
        public string Horarios { get; set; } = string.Empty; // Ej: "Lun 18:00-22:00 / Mie 19:00-21:00"
        // Propiedades para validación de correlativas
        public bool CumpleCorrelativas { get; set; } = true;
        public string? MensajeError { get; set; }
        public string Profesor { get; set; } = "Profesor aún no asignado";
        // ✅ AGREGAR ESTAS DOS PROPIEDADES:
        public int CupoDisponible { get; set; }
        public bool YaInscripto { get; set; }
        public List<DocenteComisionListadoDTO> Docentes { get; set; } = new();
    }
}