using EduSys.Shared.DTOs;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IInscripcionFinalRepository
    {
        // Obtiene todas las mesas de un periodo y evalúa la situación del alumno
        Task<List<MesaFinalOfertaDTO>> GetOfertaParaAlumnoAsync(int idAlumno, int idPeriodo);

        // Procesa la inscripción (valida ventanas operativas y reglas)
        Task<ResultadoOperacionDTO> InscribirAlumnoAsync(InscripcionFinalRequestDTO dto);

        // Permite al alumno darse de baja si la ventana sigue abierta
        Task<ResultadoOperacionDTO> CancelarInscripcionAsync(int idInscripcion, int idAlumno);

        // Obtiene las mesas a las que está anotado actualmente
        Task<List<MesaFinalOfertaDTO>> GetMisInscripcionesAsync(int idAlumno, int idPeriodo);
    }
}

