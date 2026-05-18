using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface ISoporteService
    {
        Task<SoporteTicketDTO?> CrearTicketPublicoAsync(TicketPublicoRequestDTO request);
        Task<SoporteTicketDTO?> CrearTicketInternoAsync(TicketInternoRequestDTO request);
        Task<List<SoporteTicketDTO>> GetMisTicketsAsync();
        Task<SoporteTicketDetalleDTO?> GetDetalleTicketAsync(int idTicket);
        Task<SoporteMensajeDTO?> AgregarMensajeAsync(NuevoMensajeRequestDTO request);
        Task<List<SoporteTicketDTO>> GetAllTicketsAdminAsync(string estado = "Todos", string? busqueda = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int limite = 10);
        Task<bool> CambiarEstadoTicketAsync(int idTicket, string nuevoEstado);
    }
}