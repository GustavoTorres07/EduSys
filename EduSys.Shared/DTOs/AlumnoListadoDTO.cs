namespace EduSys.Shared.DTOs
{
    public class AlumnoListadoDTO
    {
        public int IdAlumno { get; set; }
        public string NombreCompleto { get; set; } = string.Empty; // "Apellido, Nombre"
        public string Dni { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int IdSede { get; set; }
        // Datos informativos para la grilla
        public string NombrePlan { get; set; } = string.Empty;
        public string NombreCarrera { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public bool Activo { get; set; }

        public string NombreSede { get; set; } = string.Empty;
    }
}