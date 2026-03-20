namespace EduSys.Shared.DTOs
{
    public class ConstanciaInscripcionDTO
    {        public string InstitucionNombre { get; set; } = "EduSys Instituto Superior";
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public string PeriodoAcademico { get; set; } = string.Empty;
        public string AlumnoNombre { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty; 
        public List<DetalleMateriaConstanciaDTO> Materias { get; set; } = new();
    }

    public class DetalleMateriaConstanciaDTO
    {
        public string CodigoMateria { get; set; } = string.Empty; 
        public string Materia { get; set; } = string.Empty;
        public string Comision { get; set; } = string.Empty;
        public string Horarios { get; set; } = string.Empty; 
        public int AnioCursada { get; set; }
        public DateTime FechaInscripcion { get; set; }
    }
}
