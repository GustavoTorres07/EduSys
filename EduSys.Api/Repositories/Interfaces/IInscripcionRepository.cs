using EduSys.Shared.DTOs;
using EduSys.Shared.Models;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IInscripcionRepository
    {
        Task<ResultadoInscripcionDTO> InscribirAlumnoAsync(InscripcionCursadaRequestDTO dto); Task<bool> CancelarInscripcionAsync(int idInscripcion);
        Task<List<InscripcionCursada>> GetInscripcionesPorAlumnoAsync(int idAlumno, int idPeriodo);
        Task<List<InscripcionCursada>> GetInscripcionesPorComisionAsync(int idComision);

        // Validaciones públicas (por si el controlador quiere chequear antes)
        Task<bool> ValidarCorrelativasAsync(int idAlumno, int idPlanMateria);

        Task<List<ComisionDTO>> GetOfertaParaAlumnoAsync(int idAlumno, int idPeriodo);

        Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesByAlumnoAsync(int idAlumno);
        Task<ResultadoInscripcionDTO> InscribirAdminAsync(InscripcionManualDTO dto);
    }
}
