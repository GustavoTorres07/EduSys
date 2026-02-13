using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class MateriaDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio")]
        public string Codigo { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
