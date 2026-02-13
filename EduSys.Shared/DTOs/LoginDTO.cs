using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Clave { get; set; } = string.Empty;
    }
}
