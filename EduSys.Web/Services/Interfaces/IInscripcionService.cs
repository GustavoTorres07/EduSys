using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IInscripcionService
    {
        // ==========================================
        // MÉTODOS DE ALUMNO
        // ==========================================
        Task<ResultadoInscripcionDTO> InscribirAlumnoAsync(InscripcionCursadaRequestDTO dto);
        Task<List<ComisionDTO>> GetOfertaParaAlumnoAsync(int idAlumno, int idPeriodo);
        Task<bool> CancelarInscripcionAsync(int idInscripcion);

        // ==========================================
        // MÉTODOS DE ADMIN / SECRETARÍA
        // ==========================================
        Task<ResultadoInscripcionDTO> InscribirAdminAsync(InscripcionManualDTO dto);
        Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesByAlumnoAsync(int idAlumno);

        Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesAlumnoAsync(int idAlumno, int idPeriodo);
    }
}