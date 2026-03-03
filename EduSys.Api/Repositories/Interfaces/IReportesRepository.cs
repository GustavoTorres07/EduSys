using EduSys.Shared.DTOs;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IReportesRepository
    {
        // Métodos movidos desde InscripcionRepository
        Task<ConstanciaInscripcionDTO> GetDatosConstanciaAsync(int idAlumno, int idPeriodo);
        Task<List<InscripcionGlobalDTO>> GetReporteGlobalAsync(int idPeriodo, int? idCarrera);
        Task<List<AlumnoResumenInscripcionDTO>> GetAlumnosInscriptosAsync(int idPeriodo, int idCarrera, int? idSede);
        Task<CertificadoAlumnoRegularDTO> GetDatosCertificadoRegularAsync(int idAlumno, int idPeriodo);
        Task<HistoriaAcademicaDTO> GetHistoriaAcademicaAsync(int idAlumno);

        Task<ConstanciaFinalDTO?> GetDatosConstanciaFinalAsync(int idInscripcion, int idAlumno);

    }
}
