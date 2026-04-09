// 📁 NUEVO: IActasService.cs
using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IActasService
    {
        Task<bool> CerrarActaAsync(int idEvaluacion);
        Task<bool> ReabrirActaAsync(int idEvaluacion);
        Task<bool> CerrarCursadaAsync(CierreCursadaDTO dto);
        Task<bool> ReabrirComisionAsync(int idComision);
        Task<bool> ToggleCierreIndividualAsync(int idInscripcion);
        Task<List<ActaResumenDTO>> GetActasPorAlumnoAsync(int idAlumno);
        Task<bool> CerrarCursadaAsync(int idComision); // Simplificado

    }
}