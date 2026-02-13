using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IVentanaOperativaRepository
    {
        Task<List<VentanaOperativa>> GetAllAsync();
        Task<bool> CreateAsync(VentanaOperativa ventana);
        Task<bool> DeleteAsync(int id);

        // El método cerebro
        Task<bool> IsHabilitadoAsync(string tipoAccion, int idPeriodo, int? idCarrera, int? idSede);
    }
}

