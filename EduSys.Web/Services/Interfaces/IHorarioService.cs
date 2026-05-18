using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IHorarioService
    {
        Task<List<HorarioComisionDTO>> GetByComisionAsync(int idComision);
        Task<bool> CreateAsync(HorarioComisionDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<List<HorarioVisualizacionDTO>> GetVisualizacionAsync(int idPeriodo, int idCarrera, int idSede);
        Task<byte[]> DescargarPdfAsync(int idPeriodo, int idCarrera, int idSede);
        Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno);
        Task<List<HorarioVisualizacionDTO>> GetMisHorariosAsync();
    }
}