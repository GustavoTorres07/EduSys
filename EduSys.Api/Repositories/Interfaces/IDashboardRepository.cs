using EduSys.Shared.DTOs;

namespace EduSys.Api.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardDTO> GetResumenAsync();
    }
}
