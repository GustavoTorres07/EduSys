using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class AlumnoRequestDTO
    {
        public int IdAlumno { get; set; } 

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        public DateTime? FechaNacimiento { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public int IdNacionalidad { get; set; } = 1;
        public string? EstadoCivil { get; set; }
        public string? Sexo { get; set; }
        public string? LugarNacimiento { get; set; }
        public string? NombreContactoEmergencia { get; set; }
        public string? TelefonoContactoEmergencia { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public string? Legajo { get; set; } 

        [Required(ErrorMessage = "Debes asignar un plan de estudio")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un plan válido")]
        public int IdPlanActual { get; set; }
        public DateTime? FechaIngreso { get; set; } = DateTime.Now;
        public string? Ocupacion { get; set; }    
        public string? LugarTrabajo { get; set; }   
        public string? HorarioLaboral { get; set; } 
        public bool TituloSecundarioEntregado { get; set; }
        public string? Observaciones { get; set; }
        public bool? EstaBloqueado { get; set; }    
        public string? MotivoBloqueo { get; set; } 
        public bool Activo { get; set; } = true;
        public DateTime? FechaEgreso { get; set; }
        public string? UrlDniFrente { get; set; }
        public string? UrlDniDorso { get; set; }
        public string? UrlTituloSecundario { get; set; }
        public string? UrlAntecedentesPenales { get; set; }
        public string? UrlValidacionIdentidad { get; set; }
        public int IdSede { get; set; }
    }
}