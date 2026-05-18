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

// ==================================================================
// 1. COMPONENTES BASE Y LIBRERÍAS
// ==================================================================
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

// ==================================================================
// 2. CONFIGURACIÓN HTTP Y SEGURIDAD (TOKEN INTERCEPTOR)
// ==================================================================
builder.Services.AddScoped<AuthMessageHandler>();

// 💡 PRO-TIP: A futuro, puedes poner la URL en wwwroot/appsettings.json
// var apiUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7188/";

//builder.Services.AddHttpClient("Api", client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7188/"); // Reemplazar con apiUrl a futuro
//})
//.AddHttpMessageHandler<AuthMessageHandler>();

builder.Services.AddHttpClient("Api", client =>
{
    // 🚀 AHORA APUNTA A TU DOMINIO REAL EN MONSTERASP
    client.BaseAddress = new Uri("https://edusysapi.runasp.net/");
})
.AddHttpMessageHandler<AuthMessageHandler>();

// HttpClient por defecto que usará toda la app (con el token inyectado)
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

// Sistema de Autorización
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// ==================================================================
// 3. INYECCIÓN DE DEPENDENCIAS: SERVICIOS DE INTEGRACIÓN (API)
// ==================================================================

// --- Autenticación y Perfil ---
builder.Services.AddScoped<IAuthService, AuthService>();

// --- Catálogos e Infraestructura ---
builder.Services.AddScoped<ISedeService, SedeService>();
builder.Services.AddScoped<IAulaService, AulaService>();
builder.Services.AddScoped<IModalidadService, ModalidadService>();
builder.Services.AddScoped<IRegimenService, RegimenService>();
builder.Services.AddScoped<IPeriodoService, PeriodoService>();
builder.Services.AddScoped<IVentanaService, VentanaService>();
builder.Services.AddScoped<IEstadoMateriaService, EstadoMateriaService>();

// --- Académico: Carreras, Planes y Materias ---
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IMateriaService, MateriaService>();
builder.Services.AddScoped<IComisionService, ComisionService>();
builder.Services.AddScoped<IHorarioService, HorarioService>();

// --- Usuarios: Alumnos y Docentes ---
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IDocenteService, DocenteService>();
builder.Services.AddScoped<IAlumnoPortalService, AlumnoPortalService>();

// --- Transaccional: Inscripciones, Notas y Finales ---
builder.Services.AddScoped<IInscripcionService, InscripcionService>();
builder.Services.AddScoped<IInscripcionFinalService, InscripcionFinalService>();
builder.Services.AddScoped<IMesaFinalService, MesaFinalService>();
builder.Services.AddScoped<INotasService, NotasService>();

// --- Reportes y Dashboards ---
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportesService, ReportesService>();
builder.Services.AddScoped<IHistorialService, HistorialService>();

builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<INotificacionApiService, NotificacionApiService>();
builder.Services.AddScoped<IRolService, RolService>();  
builder.Services.AddScoped<IActasService, ActasService>();
builder.Services.AddScoped<ISoporteService, SoporteService>();
// ==================================================================
// 4. CONFIGURACIONES ADICIONALES Y ARRANQUE
// ==================================================================

// Requerido por algunas librerías de generación de archivos/excel/pdf en el cliente
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

await builder.Build().RunAsync();