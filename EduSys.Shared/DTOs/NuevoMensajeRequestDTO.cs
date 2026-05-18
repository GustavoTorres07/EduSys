using System.ComponentModel.DataAnnotations;

namespace EduSys.Shared.DTOs
{
    public class NuevoMensajeRequestDTO
    {
        public int IdTicket { get; set; }

        [Required(ErrorMessage = "El mensaje no puede estar vacío.")]
        public string Mensaje { get; set; } = string.Empty;

        // El Backend se encarga de saber quién es el que responde y marcar "EsRespuestaSoporte"
    }
}