using EduSys.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduSys.Web.Services.Interfaces
{
    public interface IDocenteService
    {
        // Métodos Administrativos (ABM)
        Task<List<DocenteListadoDTO>> GetDocentesAsync();
        Task<DocenteRequestDTO?> GetDocenteByIdAsync(int id);
        Task<bool> CrearDocenteAsync(DocenteRequestDTO docente);
        Task<bool> EditarDocenteAsync(DocenteRequestDTO docente);
        Task<bool> EliminarDocenteAsync(int id);

        // ✅ NUEVO: Para el Dashboard del Docente
        Task<List<ComisionDocenteDTO>> GetMisComisionesAsync();
    }
}