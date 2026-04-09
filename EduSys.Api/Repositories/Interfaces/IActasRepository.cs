using EduSys.Shared.DTOs;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IActasRepository
    {
        // 🚀 NUEVO ENFOQUE: Cierres que generan Actas Individuales
        Task<bool> CerrarActaAsync(int idEvaluacion);
        Task<bool> CerrarActaComisionAsync(int idComision);

        Task<bool> ReabrirActaAsync(int idEvaluacion);
        Task<bool> ReabrirActaComisionAsync(int idComision);
        Task<bool> ToggleCierreCursadaIndividualAsync(int idInscripcion);

        // 🚀 NUEVO ENFOQUE: Leer desde la nueva tabla ActaAlumno
        Task<List<ActaResumenDTO>> GetActasPorAlumnoAsync(int idAlumno);
    }
}