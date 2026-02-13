using EduSys.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduSys.Web.Services.Interfaces
{
    public interface IHistorialService
    {
        Task<HistoriaAcademicaDTO> GetAvanceAsync(int idAlumno);
        Task<List<PeriodoHistorialDTO>> GetCronologicoAsync(int idAlumno);
    }
}