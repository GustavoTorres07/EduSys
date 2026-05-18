using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class TicketInternoRequestDTO
    {
        [Required(ErrorMessage = "Debe seleccionar una categoría.")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "El asunto es obligatorio.")]
        [StringLength(200, ErrorMessage = "El asunto no puede superar los 200 caracteres.")]
        public string Asunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe describir su problema en el mensaje.")]
        public string Mensaje { get; set; } = string.Empty;
    }
}