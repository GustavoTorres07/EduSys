using EduSys.Api.Data;
using EduSys.Api.Helpers;
using EduSys.Api.Repositories;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Api.Services;
using EduSys.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==================================================================
// 1. CONFIGURACIÓN DE SERVICIOS (ANTES DEL BUILD)
// ==================================================================

// --- Base de Datos ---
builder.Services.AddDbContext<EduSysDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- CORS (Comunicación con Blazor) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:7157", "http://localhost:5166")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- Autenticación JWT ---
var key = builder.Configuration["Jwt:Key"];
// ⚠️ NOTA: Cambiar esta llave estática en entornos de producción reales
var keyBytes = Encoding.ASCII.GetBytes(key ?? "0VNTXYqBc5c2kY+3NyYWz/tctkkX02YxKpaKfWupiV6JYM3h8QMoNlkWAHi1fqpRn868dSOCBNBaNSLi+7F5sQ==");

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// --- Inyección de Dependencias: Helpers & Servicios Base ---
builder.Services.AddScoped<FileStorageHelper>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();

// --- Inyección de Dependencias: Repositorios Académicos ---
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<IDocenteRepository, DocenteRepository>();
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IPlanEstudioRepository, PlanEstudioRepository>();
builder.Services.AddScoped<IComisionRepository, ComisionRepository>();
builder.Services.AddScoped<IHorarioRepository, HorarioRepository>();

// --- Inyección de Dependencias: Transaccionales ---
builder.Services.AddScoped<IInscripcionRepository, InscripcionRepository>();
builder.Services.AddScoped<IInscripcionFinalRepository, InscripcionFinalRepository>();
builder.Services.AddScoped<IMesaFinalRepository, MesaFinalRepository>();
builder.Services.AddScoped<INotasRepository, NotasRepository>();
builder.Services.AddScoped<ISolicitudIngresoRepository, SolicitudIngresoRepository>();

// --- Inyección de Dependencias: Consultas y Reportes ---
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IReportesRepository, ReportesRepository>();
builder.Services.AddScoped<IHistorialAcademicoRepository, HistorialAcademicoRepository>();
builder.Services.AddScoped<IAlumnoPortalRepository, AlumnoPortalRepository>();

// --- Inyección de Dependencias: Configuración e Infraestructura ---
builder.Services.AddScoped<IModalidadRepository, ModalidadRepository>();
builder.Services.AddScoped<IRegimenRepository, RegimenRepository>();
builder.Services.AddScoped<IEstadoMateriaRepository, EstadoMateriaRepository>();
builder.Services.AddScoped<IPeriodoRepository, PeriodoRepository>();
builder.Services.AddScoped<IVentanaOperativaRepository, VentanaOperativaRepository>();
builder.Services.AddScoped<IInfrastructureRepository, InfrastructureRepository>();
builder.Services.AddScoped<IAulaRepository, AulaRepository>();

builder.Services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();

builder.Services.AddScoped<IRolRepository, RolRepository>();

builder.Services.AddHostedService<EduSys.Api.Workers.NotificacionesWorker>();

builder.Services.AddControllers();

// ==================================================================
// 2. CONSTRUCCIÓN DE LA APLICACIÓN
// ==================================================================
var app = builder.Build();

// Configuración global para generación de PDFs
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ==================================================================
// 3. PIPELINE DE MIDDLEWARE (ORDEN CRÍTICO)
// ==================================================================

app.UseHttpsRedirection();

app.UseCors("PermitirBlazor"); // 1. CORS
app.UseStaticFiles();          // 2. Archivos estáticos (imágenes locales, etc.)

app.UseAuthentication();       // 3. Auth (¿Quién eres?)
app.UseAuthorization();        // 4. Autorización (¿Qué puedes hacer?)

app.MapControllers();

app.Run();