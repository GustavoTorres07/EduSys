using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class SoporteService : ISoporteService
    {
        private readonly HttpClient _http;

        public SoporteService(HttpClient http)
        {
            _http = http;
        }

        public async Task<SoporteTicketDTO?> CrearTicketPublicoAsync(TicketPublicoRequestDTO request)
        {
            var response = await _http.PostAsJsonAsync("api/Soporte/publico", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RespuestaCreacionTicket>();
                return result?.Ticket;
            }
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }

        public async Task<SoporteTicketDTO?> CrearTicketInternoAsync(TicketInternoRequestDTO request)
        {
            var response = await _http.PostAsJsonAsync("api/Soporte/interno", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<RespuestaCreacionTicket>();
            return result?.Ticket;
        }

        public async Task<List<SoporteTicketDTO>> GetMisTicketsAsync()
        {
            return await _http.GetFromJsonAsync<List<SoporteTicketDTO>>("api/Soporte/mis-tickets")
                   ?? new List<SoporteTicketDTO>();
        }

        public async Task<SoporteTicketDetalleDTO?> GetDetalleTicketAsync(int idTicket)
        {
            return await _http.GetFromJsonAsync<SoporteTicketDetalleDTO>($"api/Soporte/{idTicket}");
        }

        public async Task<SoporteMensajeDTO?> AgregarMensajeAsync(NuevoMensajeRequestDTO request)
        {
            var response = await _http.PostAsJsonAsync("api/Soporte/mensaje", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SoporteMensajeDTO>();
        }

        public async Task<List<SoporteTicketDTO>> GetAllTicketsAdminAsync(string estado = "Todos", string? busqueda = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int limite = 10)
        {
            var url = $"api/Soporte/admin/todos?estado={estado}&limite={limite}";

            if (!string.IsNullOrWhiteSpace(busqueda))
                url += $"&busqueda={Uri.EscapeDataString(busqueda)}";

            if (fechaDesde.HasValue)
                url += $"&fechaDesde={fechaDesde.Value:yyyy-MM-dd}";

            if (fechaHasta.HasValue)
                url += $"&fechaHasta={fechaHasta.Value:yyyy-MM-dd}";

            return await _http.GetFromJsonAsync<List<SoporteTicketDTO>>(url) ?? new List<SoporteTicketDTO>();
        }

        public async Task<bool> CambiarEstadoTicketAsync(int idTicket, string nuevoEstado)
        {
            var response = await _http.PutAsJsonAsync($"api/Soporte/admin/{idTicket}/estado", nuevoEstado);
            return response.IsSuccessStatusCode;
        }

        // Clase auxiliar para mapear la respuesta del backend al crear un ticket
        private class RespuestaCreacionTicket
        {
            public string Message { get; set; } = string.Empty;
            public SoporteTicketDTO Ticket { get; set; } = new SoporteTicketDTO();
        }
    }
}