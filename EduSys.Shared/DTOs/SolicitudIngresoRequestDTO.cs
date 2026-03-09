using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class SolicitudIngresoRequestDTO
    {
        // --- DATOS PERSONALES ---
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public DateTime? FechaNacimiento { get; set; } // <--- AGREGAR ESTO

        public string? Telefono { get; set; } 
        public string? Direccion { get; set; }

        // --- CARRERA ELEGIDA ---
        [Required(ErrorMessage = "Debes seleccionar una carrera.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una carrera válida.")]
        public int IdCarreraInteres { get; set; }

        // --- ARCHIVOS (Base64) ---
        // Estos strings contendrán el archivo codificado que viene del InputFile de Blazor.

        [Required(ErrorMessage = "La foto de perfil es obligatoria.")]
        public string FotoPerfilBase64 { get; set; } = string.Empty;

        [Required(ErrorMessage = "La foto del frente del DNI es obligatoria.")]
        public string FotoDniFrenteBase64 { get; set; } = string.Empty;

        [Required(ErrorMessage = "La foto del dorso del DNI es obligatoria.")]
        public string FotoDniDorsoBase64 { get; set; } = string.Empty;

        [Required(ErrorMessage = "La foto sosteniendo tu DNI es obligatoria.")]
        public string FotoSosteniendoDniBase64 { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título secundario (o constancia) es obligatorio.")]
        public string TituloSecundarioBase64 { get; set; } = string.Empty;

        // Opcional o Requerido según tu regla de negocio (lo dejo requerido por seguridad)
        [Required(ErrorMessage = "El certificado de antecedentes es obligatorio.")]
        public string AntecedentesPenalesBase64 { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una sede.")]
        public int IdSede { get; set; }
    }
}
