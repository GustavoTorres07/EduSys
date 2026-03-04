using EduSys.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduSys.Web.Services.Interfaces
{
    public interface IReportesService
    {
        // Reportes Administrativos (Globales)
        Task<List<InscripcionGlobalDTO>> GetInscripcionesGlobalAsync(int idPeriodo, int? idCarrera);
        Task<List<AlumnoResumenInscripcionDTO>> GetAlumnosInscriptosAsync(int idPeriodo, int idCarrera, int? idSede);

        // Descargas PDF (Alumno)
        Task<byte[]> DescargarConstanciaInscripcionPdfAsync(int idAlumno, int idPeriodo);
        Task<byte[]> DescargarHorarioPdfAsync(int idPeriodo, int idCarrera, int idSede);

        // Consultas de Horarios
        Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno);

        Task<byte[]> DescargarCertificadoRegularPdfAsync(int idAlumno, int idPeriodo);

        Task<HistoriaAcademicaDTO> GetHistoriaAcademicaAsync(int idAlumno);
        Task<byte[]> DescargarConstanciaFinalAsync(int idInscripcion);

        Task<byte[]> DescargarAnaliticoProvisorioAsync();
    }
}