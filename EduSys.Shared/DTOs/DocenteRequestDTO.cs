using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class DocenteRequestDTO
    {
        public int IdDocente { get; set; } // 0 si es nuevo

        // --- DATOS PERSONALES (Tabla Usuario) ---
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        public string? Sexo { get; set; }
        public string? EstadoCivil { get; set; }
        public int IdNacionalidad { get; set; } = 1;

        // ✅ NUEVOS CAMPOS AGREGADOS
        public string? LugarNacimiento { get; set; }
        public string? NombreContactoEmergencia { get; set; }
        public string? TelefonoContactoEmergencia { get; set; }

        // --- DATOS ESPECÍFICOS DOCENTE (Tabla Docente) ---

        public string? Legajo { get; set; }

        [Required(ErrorMessage = "El título académico es obligatorio")]
        public string TituloAcademico { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}