using Blazored.LocalStorage;
using EduSys.Web;
using EduSys.Web.Auth;
using EduSys.Web.Services;
using EduSys.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Text;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. MudBlazor
builder.Services.AddMudServices();

// 2. LocalStorage
builder.Services.AddBlazoredLocalStorage();

// 3. Handler que agrega el JWT automáticamente
builder.Services.AddScoped<AuthMessageHandler>();

// 4. HttpClient configurado con el Handler
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7188/");
})
.AddHttpMessageHandler<AuthMessageHandler>();

// 5. HttpClient por defecto que usará toda la app
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

// 6. Sistema de Autorización
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// 7. Servicios de la aplicación
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<ISedeService, SedeService>();
builder.Services.AddScoped<IModalidadService, ModalidadService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IMateriaService, MateriaService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IRegimenService, RegimenService>();
builder.Services.AddScoped<IPeriodoService, PeriodoService>();
builder.Services.AddScoped<IComisionService, ComisionService>();
builder.Services.AddScoped<IHorarioService, HorarioService>();
builder.Services.AddScoped<IAulaService, AulaService>();
builder.Services.AddScoped<HorarioPdfService>();
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IDocenteService, DocenteService>();
builder.Services.AddScoped<IInscripcionService, InscripcionService>();
builder.Services.AddScoped<IVentanaService, VentanaService>();
builder.Services.AddScoped<IReportesService, ReportesService>();
builder.Services.AddScoped<IHistorialService, HistorialService>();
builder.Services.AddScoped<INotasService, NotasService>();
builder.Services.AddScoped<IAlumnoPortalService, AlumnoPortalService>();
builder.Services.AddScoped<IMesaFinalService, MesaFinalService>();
builder.Services.AddScoped<IInscripcionFinalService, InscripcionFinalService>();
builder.Services.AddScoped<IEstadoMateriaService, EstadoMateriaService>();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

await builder.Build().RunAsync();