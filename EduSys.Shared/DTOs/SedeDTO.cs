using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class SedeDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = null!;

        public string? Direccion { get; set; }
        public string? CodigoPostal { get; set; }
        public bool Activo { get; set; } = true;

        // Campo calculado para mostrar en la tabla
        public int CantidadAulas { get; set; }
    }
}