using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IInscripcionService
    {
        // Métodos de Alumno
        Task<ResultadoInscripcionDTO> InscribirAlumnoAsync(InscripcionCursadaRequestDTO dto);
        Task<List<ComisionDTO>> GetOfertaParaAlumnoAsync(int idAlumno, int idPeriodo);
        Task<bool> CancelarInscripcionAsync(int idInscripcion);

        // Métodos de Admin / Secretaría
        Task<ResultadoInscripcionDTO> InscribirAdminAsync(InscripcionManualDTO dto);
        Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesByAlumnoAsync(int idAlumno);
        Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesAlumnoAsync(int idAlumno, int idPeriodo);
    }
}