namespace EduSys.Shared.DTOs
{
    public class InscripcionManualDTO
    {
        public int IdAlumno { get; set; }
        public int IdComision { get; set; }
        public bool IgnorarCorrelativas { get; set; } = false;
        public bool IgnorarCupo { get; set; } = false;
        public bool IgnorarVentana { get; set; } = true; 
    }
}