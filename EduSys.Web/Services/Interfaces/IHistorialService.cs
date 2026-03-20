using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IHistorialService
    {
        // ✅ Tipado nulable para proteger la vista en caso de error
        Task<HistoriaAcademicaDTO?> GetAvanceAsync(int idAlumno);
        Task<List<PeriodoHistorialDTO>> GetCronologicoAsync(int idAlumno);
    }
}