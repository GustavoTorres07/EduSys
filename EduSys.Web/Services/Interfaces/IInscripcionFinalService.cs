using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IInscripcionFinalService
    {
        Task<List<MesaFinalOfertaDTO>> GetOfertaAsync(int idAlumno, int idPeriodo);
        Task<List<MesaFinalOfertaDTO>> GetMisInscripcionesAsync(int idAlumno, int idPeriodo);
        Task<ResultadoOperacionDTO> InscribirAsync(InscripcionFinalRequestDTO dto);
        Task<ResultadoOperacionDTO> CancelarInscripcionAsync(int idInscripcion, int idAlumno);
    }
}