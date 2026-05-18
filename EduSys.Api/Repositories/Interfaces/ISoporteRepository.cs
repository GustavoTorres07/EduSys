using EduSys.Shared.DTOs;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface ISoporteRepository
    {
        // Utilidad para el Ticket Público
        Task<int?> ObtenerIdUsuarioPorIdentificacionAsync(string identificacion);

        // ABM y Gestión
        Task<SoporteTicketDTO> CrearTicketAsync(int idUsuario, string categoria, string asunto, string mensaje);
        Task<SoporteMensajeDTO> AgregarMensajeAsync(int idTicket, int idUsuario, string mensaje, bool esRespuestaSoporte);
        Task<bool> CambiarEstadoTicketAsync(int idTicket, string nuevoEstado);

        // Consultas
        Task<List<SoporteTicketDTO>> GetTicketsPorUsuarioAsync(int idUsuario);
        Task<List<SoporteTicketDTO>> GetTodosLosTicketsAsync(string? estado = null, string? busqueda = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int limite = 10);
        Task<SoporteTicketDetalleDTO?> GetTicketDetalleAsync(int idTicket);

        // Validación de pertenencia
        Task<bool> EsTicketDelUsuarioAsync(int idTicket, int idUsuario);
    }
}