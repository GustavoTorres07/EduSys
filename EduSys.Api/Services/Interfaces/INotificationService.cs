namespace EduSys.Api.Services.Interfaces
{
    public interface INotificationService
    {
        Task NotificarCierreActaAsync(int idEvaluacion, string nombreExamen);
    }
}