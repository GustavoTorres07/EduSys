using System;
namespace EduSys.Shared.DTOs
{
    public class AlumnoDTO
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public string Legajo { get; set; } = string.Empty;
        public string EstadoAcademico { get; set; } = "Activo";
        public bool Activo { get; set; }
        public int IdPlanActual { get; set; }
        public string NombrePlan { get; set; } = string.Empty;
        public int IdCarrera { get; set; }
        public string NombreCarrera { get; set; } = string.Empty;
        public int IdSede { get; set; }
        public string NombreSede { get; set; } = string.Empty;
    }
}