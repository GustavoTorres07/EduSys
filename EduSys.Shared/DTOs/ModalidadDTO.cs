using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class ModalidadDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public string? Codigo { get; set; } 
        public bool Activo { get; set; } = true;
    }
}
