using EduSys.Shared.DTOs;
using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IHorarioRepository
    {
        Task<List<HorarioComision>> GetByComisionAsync(int idComision);
        Task<bool> CreateAsync(HorarioComision horario);
        Task<bool> DeleteAsync(int id);

        // Método clave para evitar conflictos
        Task<bool> ValidarSuperposicionAsync(int idAula, string dia, TimeSpan inicio, TimeSpan fin);

        Task<List<HorarioVisualizacionDTO>> GetHorariosByCarreraAndPeriodoAsync(int idPeriodo, int idCarrera, int idSede); // <-- Agregamos idSede

        Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno);
    }
}
