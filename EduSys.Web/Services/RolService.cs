using System.Net.Http.Json;
using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;

namespace EduSys.Web.Services;

public class RolService : IRolService
{
    private readonly HttpClient _http;
    public RolService(HttpClient http) => _http = http;

    public async Task<List<RolRequestDTO>> GetAllAsync() =>
        await _http.GetFromJsonAsync<List<RolRequestDTO>>("api/roles") ?? new();

    public async Task<RolRequestDTO?> GetByIdAsync(int id) =>
        await _http.GetFromJsonAsync<RolRequestDTO>($"api/roles/{id}");

    public async Task<bool> GuardarAsync(RolRequestDTO dto)
    {
        var response = await _http.PostAsJsonAsync("api/roles", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/roles/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<PermisoDTO>> GetCatalogoPermisosAsync() =>
        await _http.GetFromJsonAsync<List<PermisoDTO>>("api/roles/permisos") ?? new();
}