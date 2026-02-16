namespace EduSys.Shared.DTOs
{
    public class InscripcionManualDTO
    {
        public int IdAlumno { get; set; }
        public int IdComision { get; set; }

        // Flags para ignorar validaciones (Solo para Admins)
        public bool IgnorarCorrelativas { get; set; } = false;
        public bool IgnorarCupo { get; set; } = false;
        public bool IgnorarVentana { get; set; } = true; // Por defecto Admin ignora fechas
    }
}