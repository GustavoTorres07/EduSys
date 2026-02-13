using EduSys.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IHistorialAcademicoRepository
    {
        // 1. Para la pestaña "Avance de Carrera" (Analítico)
        Task<HistoriaAcademicaDTO> GetAvanceCarreraAsync(int idAlumno);

        // 2. Para la pestaña "Historial por Período" (Tarjetas agrupadas por ciclo)
        Task<List<PeriodoHistorialDTO>> GetHistorialCronologicoAsync(int idAlumno);
    }
}