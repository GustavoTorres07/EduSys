using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class RegimenDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
