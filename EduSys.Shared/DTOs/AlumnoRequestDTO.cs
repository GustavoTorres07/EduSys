using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class AlumnoRequestDTO
    {
        public int IdAlumno { get; set; } // 0 si es nuevo

        // ==========================================
        // 1. DATOS DE IDENTIDAD (Tabla Usuario)
        // ==========================================

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

        // ✅ AGREGAR ESTOS DOS QUE FALTABAN (Están en tu DB Usuario)
        public string? Sexo { get; set; }
        public string? LugarNacimiento { get; set; }

        public string? NombreContactoEmergencia { get; set; }
        public string? TelefonoContactoEmergencia { get; set; }
        public string? FotoPerfilUrl { get; set; }

        // ==========================================
        // 2. DATOS ACADÉMICOS
        // ==========================================

        public string? Legajo { get; set; } // Puede venir vacío y se genera en el backend

        [Required(ErrorMessage = "Debes asignar un plan de estudio")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un plan válido")]
        public int IdPlanActual { get; set; }

        public DateTime? FechaIngreso { get; set; } = DateTime.Now;

        // ==========================================
        // 3. DATOS ADMIN
        // ==========================================
        public string? Ocupacion { get; set; }      // ✅ Agregado
        public string? LugarTrabajo { get; set; }   // ✅ Agregado
        public string? HorarioLaboral { get; set; } // ✅ Agregado

        public bool TituloSecundarioEntregado { get; set; }
        public string? Observaciones { get; set; }

        public bool? EstaBloqueado { get; set; }    // ✅ Agregado (opcional para editar)
        public string? MotivoBloqueo { get; set; }  // ✅ Agregado (opcional para editar)
        public bool Activo { get; set; } = true;
        public DateTime? FechaEgreso { get; set; }
        // ==========================================
        // 4. LEGAJO DIGITAL (URLs)
        // ==========================================
        public string? UrlDniFrente { get; set; }
        public string? UrlDniDorso { get; set; }
        public string? UrlTituloSecundario { get; set; }
        public string? UrlAntecedentesPenales { get; set; }
        public string? UrlValidacionIdentidad { get; set; }

        public int IdSede { get; set; }
    }
}