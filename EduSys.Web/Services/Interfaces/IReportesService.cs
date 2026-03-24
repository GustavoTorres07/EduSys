using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IReportesService
    {
        // Reportes Administrativos (Globales)
        Task<List<InscripcionGlobalDTO>> GetInscripcionesGlobalAsync(int idPeriodo, int? idCarrera);
        Task<List<AlumnoResumenInscripcionDTO>> GetAlumnosInscriptosAsync(int idPeriodo, int idCarrera, int? idSede);

        // Descargas PDF (Alumno / Administrativo)
        Task<byte[]> DescargarConstanciaInscripcionPdfAsync(int idAlumno, int idPeriodo);
        Task<byte[]> DescargarHorarioPdfAsync(int idPeriodo, int idCarrera, int idSede);
        Task<byte[]> DescargarCertificadoRegularPdfAsync(int idAlumno, int idPeriodo);
        Task<byte[]> DescargarConstanciaFinalAsync(int idInscripcion);
        Task<byte[]> DescargarAnaliticoProvisorioAsync();
        // Consultas Generales
        Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno);
        Task<HistoriaAcademicaDTO?> GetHistoriaAcademicaAsync(int idAlumno); // ✅ Nulable para mayor seguridad
    }
}