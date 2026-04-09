using EduSys.Shared.DTOs;

namespace EduSys.Api.Services.Interfaces
{
    public interface IPdfReportService
    {
        byte[] GenerarConstanciaInscripcion(ConstanciaInscripcionDTO data);
        byte[] GenerarConstanciaInscripcionFinal(ConstanciaFinalDTO datos);
        byte[] GenerarAnaliticoProvisorio(HistoriaAcademicaDTO datos);
        byte[] GenerarActaIndividual(ActaIndividualDTO datos);
    }
}
