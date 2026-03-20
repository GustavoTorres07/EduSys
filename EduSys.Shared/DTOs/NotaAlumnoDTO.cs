namespace EduSys.Shared.DTOs
{
    public class NotaAlumnoDTO
    {
        public int IdInscripcion { get; set; }
        public string AlumnoNombre { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public Dictionary<int, decimal?> Notas { get; set; } = new();
        public bool CursadaCerrada { get; set; }
        public decimal? Promedio { get; set; }
        public string Estado { get; set; } = "Cursando";
    }
}