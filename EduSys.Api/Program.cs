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
// 1. CONFIGURACIÓN DE SERVICIOS (TODO ESTO VA ANTES DEL BUILD)
// ==================================================================

// Configurar DB
builder.Services.AddDbContext<EduSysDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar CORS (Vital para que Blazor pueda hablar con la API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirBlazor", policy =>
    {
        // AQUÍ PONEMOS EL PUERTO QUE VIMOS EN TU LAUNCHSETTINGS DEL FRONTEND
        policy.WithOrigins("https://localhost:7157", "http://localhost:5166")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar Autenticación JWT
var key = builder.Configuration["Jwt:Key"];
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

// Registrar Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IModalidadRepository, ModalidadRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IPlanEstudioRepository, PlanEstudioRepository>();
builder.Services.AddScoped<IRegimenRepository, RegimenRepository>();
builder.Services.AddScoped<IInfrastructureRepository, InfrastructureRepository>();
builder.Services.AddScoped<IPeriodoRepository, PeriodoRepository>();
builder.Services.AddScoped<IComisionRepository, ComisionRepository>();
builder.Services.AddScoped<IHorarioRepository, HorarioRepository>();
builder.Services.AddScoped<IAulaRepository, AulaRepository>();
builder.Services.AddScoped<FileStorageHelper>();
builder.Services.AddScoped<ISolicitudIngresoRepository, SolicitudIngresoRepository>();
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDocenteRepository, DocenteRepository>();
builder.Services.AddScoped<IInscripcionRepository, InscripcionRepository>();
builder.Services.AddScoped<IVentanaOperativaRepository, VentanaOperativaRepository>();
builder.Services.AddScoped<IReportesRepository, ReportesRepository>();
builder.Services.AddScoped<IHistorialAcademicoRepository, HistorialAcademicoRepository>();
builder.Services.AddScoped<INotasRepository, NotasRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAlumnoPortalRepository, AlumnoPortalRepository>();
builder.Services.AddScoped<IMesaFinalRepository, MesaFinalRepository>();
builder.Services.AddScoped<IInscripcionFinalRepository, InscripcionFinalRepository>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped<IEstadoMateriaRepository, EstadoMateriaRepository>();

builder.Services.AddControllers();

// ==================================================================
// 2. CONSTRUCCIÓN DE LA APLICACIÓN
// ==================================================================
var app = builder.Build();

// 1. Agrega esto justo después de "var app = builder.Build();" o después de "var builder = WebApplication.CreateBuilder(args);"
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ==================================================================
// 3. PIPELINE DE MIDDLEWARE (ORDEN ES CRÍTICO)
// ==================================================================

app.UseHttpsRedirection();

app.UseCors("PermitirBlazor"); // 1. CORS primero
app.UseStaticFiles();
app.UseAuthentication();       // 2. Auth (¿Quién eres?)
app.UseAuthorization();        // 3. Autorización (¿Qué puedes hacer?)

app.MapControllers();

app.Run();