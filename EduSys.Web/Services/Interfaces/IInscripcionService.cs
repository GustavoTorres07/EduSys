using EduSys.Shared.DTOs;

public interface IInscripcionService
{
    Task<ResultadoInscripcionDTO> InscribirAsync(InscripcionCursadaRequestDTO dto);

    Task<bool> CancelarInscripcionAsync(int idInscripcion);

    Task<List<InscripcionCursadaListadoDTO>> GetInscripcionesAlumnoAsync(int idAlumno, int idPeriodo);

    Task<List<ComisionDTO>> GetOfertaInscripcionAsync(int idAlumno, int idPeriodo);

    Task<ResultadoInscripcionDTO> InscribirAdminAsync(InscripcionManualDTO dto);
}