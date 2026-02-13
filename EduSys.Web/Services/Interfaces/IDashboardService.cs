using EduSys.Shared.DTOs;

namespace EduSys.Web.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDTO> GetResumenAsync();
    }
}
