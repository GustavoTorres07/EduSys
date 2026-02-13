using EduSys.Api.Data;
using EduSys.Api.Helpers;
using EduSys.Api.Repositories.Interfaces;
using EduSys.Api.Services.Interfaces;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace EduSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolicitudesController : ControllerBase
    {
        private readonly ISolicitudIngresoRepository _solicitudRepo;
        private readonly ICarreraRepository _carreraRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly EduSysDbContext _context;
        private readonly FileStorageHelper _fileHelper;
        private readonly IEmailService _emailService;

        public SolicitudesController(
            ISolicitudIngresoRepository solicitudRepo,
            ICarreraRepository carreraRepo,
            IUsuarioRepository usuarioRepo,
            EduSysDbContext context,
            FileStorageHelper fileHelper,
            IEmailService emailService)
        {
            _solicitudRepo = solicitudRepo;
            _carreraRepo = carreraRepo;
            _usuarioRepo = usuarioRepo;
            _context = context;
            _fileHelper = fileHelper;
            _emailService = emailService;
        }

        // ---------------------------------------------------------
        // 1. CREAR SOLICITUD (Público)
        // ---------------------------------------------------------
        [HttpPost("solicitar")]
        [AllowAnonymous]
        public async Task<IActionResult> CrearSolicitud([FromBody] SolicitudIngresoRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validar si ya existe
            if (await _solicitudRepo.ExistePendienteAsync(dto.Dni, dto.IdCarreraInteres))
                return BadRequest("Ya tienes una solicitud pendiente de revisión para esta carrera.");

            try
            {
                var carreraInfo = await _carreraRepo.GetByIdAsync(dto.IdCarreraInteres);
                string nombreCarrera = carreraInfo?.Nombre ?? "Carrera seleccionada";

                // Subida de archivos (Mantenemos tu lógica existente)
                string dniFolder = dto.Dni.Trim();
                string categoria = "documentacion_ingreso";

                var urlPerfil = await _fileHelper.GuardarArchivoAsync(dto.FotoPerfilBase64, categoria, dniFolder, "perfil");
                var urlSosteniendo = await _fileHelper.GuardarArchivoAsync(dto.FotoSosteniendoDniBase64, categoria, dniFolder, "prueba_vida_dni");
                var urlDniFrente = await _fileHelper.GuardarArchivoAsync(dto.FotoDniFrenteBase64, categoria, dniFolder, "dni_frente");
                var urlDniDorso = await _fileHelper.GuardarArchivoAsync(dto.FotoDniDorsoBase64, categoria, dniFolder, "dni_dorso");
                var urlTitulo = await _fileHelper.GuardarArchivoAsync(dto.TituloSecundarioBase64, categoria, dniFolder, "titulo_secundario");
                var urlPenales = await _fileHelper.GuardarArchivoAsync(dto.AntecedentesPenalesBase64, categoria, dniFolder, "antecedentes_penales");

                var nuevaSolicitud = new SolicitudIngreso
                {
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    Dni = dto.Dni,
                    FechaNacimiento = dto.FechaNacimiento,
                    Email = dto.Email,
                    Telefono = dto.Telefono,
                    Direccion = dto.Direccion,

                    IdCarreraInteres = dto.IdCarreraInteres,
                    // ✅ NUEVO: Guardamos la Sede elegida
                    IdSede = dto.IdSede,

                    RutaFotoPerfil = urlPerfil,
                    RutaFotoSosteniendoDNI = urlSosteniendo,
                    RutaFotoDniFrente = urlDniFrente,
                    RutaFotoDniDorso = urlDniDorso,
                    RutaTituloSecundario = urlTitulo,
                    RutaAntecedentesPenales = urlPenales,
                    Estado = "Pendiente",
                    FechaSolicitud = DateTime.Now
                };

                await _solicitudRepo.CrearAsync(nuevaSolicitud);

                // --- EMAIL CONFIRMACIÓN ---
                try
                {
                    string cuerpoEmail = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                            <div style='background-color: #456990; color: white; padding: 20px; text-align: center;'>
                                <h1 style='margin: 0; font-size: 24px;'>EduSys</h1>
                                <p style='margin: 5px 0 0; font-size: 14px; opacity: 0.9;'>Departamento de Admisiones</p>
                            </div>
                            <div style='padding: 30px; background-color: #ffffff;'>
                                <h2 style='color: #334155; margin-top: 0;'>¡Hola, {dto.Nombre} {dto.Apellido}!</h2>
                                <p style='color: #64748b; line-height: 1.6;'>Nos complace informarte que hemos recibido correctamente tu solicitud de inscripción.</p>
                                <div style='background-color: #f0fdf4; border-left: 4px solid #49BEAA; padding: 20px; margin: 25px 0;'>
                                    <p style='margin: 0; font-size: 12px; font-weight: bold; color: #64748b; text-transform: uppercase;'>CARRERA SOLICITADA</p>
                                    <p style='margin: 5px 0 0; font-size: 18px; font-weight: bold; color: #0f172a;'>{nombreCarrera}</p>
                                </div>
                            </div>
                        </div>";

                    await _emailService.SendEmailAsync(dto.Email, "Solicitud Recibida - EduSys", cuerpoEmail);
                }
                catch { }

                return Ok(new { mensaje = "¡Solicitud recibida correctamente! Revisa tu correo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // ---------------------------------------------------------
        // 2. LISTAR PENDIENTES
        // ---------------------------------------------------------
        [HttpGet("pendientes")]
        public async Task<ActionResult<List<SolicitudIngresoDTO>>> GetPendientes()
        {
            var lista = await _solicitudRepo.GetPendientesAsync();

            var listaDto = lista.Select(x => new SolicitudIngresoDTO
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Apellido = x.Apellido,
                Dni = x.Dni,
                Email = x.Email,
                Telefono = x.Telefono,
                FechaSolicitud = x.FechaSolicitud ?? DateTime.Now,
                FechaProcesado = x.FechaProcesado,
                Estado = x.Estado,
                RutaFotoPerfil = x.RutaFotoPerfil,
                IdCarreraInteres = x.IdCarreraInteres,

                NombreCarrera = x.IdCarreraInteresNavigation?.Nombre ?? "-",
                // ✅ NUEVO: Mostramos el nombre de la Sede si existe
                NombreSede = x.IdSedeNavigation?.Nombre ?? "-",

                FechaNacimiento = x.FechaNacimiento
            }).ToList();

            return Ok(listaDto);
        }

        // ---------------------------------------------------------
        // 3. OBTENER DETALLE
        // ---------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<SolicitudIngresoDTO>> GetSolicitudById(int id)
        {
            var s = await _solicitudRepo.GetByIdAsync(id);
            if (s == null) return NotFound("Solicitud no encontrada.");

            var dto = new SolicitudIngresoDTO
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Apellido = s.Apellido,
                Dni = s.Dni,
                Email = s.Email,
                Telefono = s.Telefono,
                Direccion = s.Direccion,
                FechaSolicitud = s.FechaSolicitud ?? DateTime.Now,
                Estado = s.Estado,
                RutaFotoPerfil = s.RutaFotoPerfil,
                IdCarreraInteres = s.IdCarreraInteres,
                NombreCarrera = s.IdCarreraInteresNavigation?.Nombre ?? "-",
                // ✅ NUEVO: Mostramos Sede
                IdSede = s.IdSede ?? 0,
                NombreSede = s.IdSedeNavigation?.Nombre ?? "-",

                FechaNacimiento = s.FechaNacimiento,
                RutaTituloSecundario = s.RutaTituloSecundario,
                FechaProcesado = s.FechaProcesado,
                RutaAntecedentesPenales = s.RutaAntecedentesPenales,
                RutaFotoSosteniendoDNI = s.RutaFotoSosteniendoDNI,
                RutaFotoDniFrente = s.RutaFotoDniFrente,
                RutaFotoDniDorso = s.RutaFotoDniDorso,
                ObservacionAdmin = s.ObservacionAdmin
            };

            return Ok(dto);
        }

        // ---------------------------------------------------------
        // 4. PROCESAR (APROBAR / RECHAZAR)
        // ---------------------------------------------------------
        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarSolicitud([FromBody] ProcesarSolicitudDTO decision)
        {
            var solicitud = await _solicitudRepo.GetByIdAsync(decision.SolicitudId);
            if (solicitud == null) return NotFound("Solicitud no encontrada");

            if (solicitud.Estado != "Pendiente")
                return BadRequest($"La solicitud ya fue procesada anteriormente ({solicitud.Estado}).");

            try
            {
                // === RECHAZO ===
                if (!decision.EsAprobado)
                {
                    solicitud.Estado = "Rechazada";
                    solicitud.ObservacionAdmin = decision.MotivoRechazo;
                    solicitud.FechaProcesado = DateTime.Now;
                    await _solicitudRepo.UpdateAsync(solicitud);

                    try
                    {
                        await _emailService.SendEmailAsync(solicitud.Email, "EduSys - Estado de Solicitud",
                            $"<p>Hola {solicitud.Nombre}, lamentamos informarte que tu solicitud ha sido rechazada.</p><p><strong>Motivo:</strong> {decision.MotivoRechazo}</p>");
                    }
                    catch { }

                    return Ok(new { message = "Solicitud rechazada correctamente." });
                }

                // === APROBACIÓN ===
                if (await _usuarioRepo.ExisteEmailAsync(solicitud.Email))
                    return BadRequest("El email del aspirante ya está registrado como usuario en el sistema.");

                string nuevoLegajo = $"{DateTime.Now.Year}-{solicitud.Dni}";

                DateOnly? fechaNacimientoDateOnly = null;
                if (solicitud.FechaNacimiento.HasValue)
                {
                    fechaNacimientoDateOnly = DateOnly.FromDateTime(solicitud.FechaNacimiento.Value);
                }

                // 1. CREAR USUARIO
                var nuevoUsuario = new Usuario
                {
                    Nombre = solicitud.Nombre,
                    Apellido = solicitud.Apellido,
                    Dni = solicitud.Dni,
                    Email = solicitud.Email,
                    Telefono = solicitud.Telefono,
                    Direccion = solicitud.Direccion,
                    FechaNacimiento = fechaNacimientoDateOnly,
                    FotoPerfilUrl = solicitud.RutaFotoPerfil,
                    ClaveHash = BCrypt.Net.BCrypt.HashPassword(solicitud.Dni),
                    IdRol = 5, // Alumno
                    IdNacionalidad = 1,
                    Activo = true,
                    FechaRegistro = DateTime.Now,
                    DebeCambiarPass = true
                };

                var usuarioCreado = await _usuarioRepo.CrearAsync(nuevoUsuario);

                // 2. CREAR ALUMNO
                var nuevoAlumno = new Alumno
                {
                    IdUsuario = usuarioCreado.Id,
                    Legajo = nuevoLegajo,
                    IdPlanActual = 1, // Aquí podrías buscar el plan vigente de la carrera
                    EstadoAcademico = "Activo",
                    Activo = true,
                    EstaBloqueado = false,
                    TituloSecundarioEntregado = !string.IsNullOrEmpty(solicitud.RutaTituloSecundario),
                    Observaciones = "Alta automática desde web.",
                    FechaIngreso = DateOnly.FromDateTime(DateTime.Now),

                    // ✅ IMPORTANTE: Aquí podríamos asignar la Sede si la tabla Alumno tuviera IdSedePreferida,
                    // pero usualmente la sede se define al inscribirse a cursadas.
                    // Si tu tabla Alumno tiene IdSede, asígnalo aquí:
                    // IdSede = solicitud.IdSede,

                    UrlDniFrente = solicitud.RutaFotoDniFrente,
                    UrlDniDorso = solicitud.RutaFotoDniDorso,
                    UrlTituloSecundario = solicitud.RutaTituloSecundario,
                    UrlAntecedentesPenales = solicitud.RutaAntecedentesPenales,
                    UrlValidacionIdentidad = solicitud.RutaFotoSosteniendoDNI,
                    IdSede = solicitud.IdSede
                };

                _context.Alumnos.Add(nuevoAlumno);
                await _context.SaveChangesAsync();

                // 3. ACTUALIZAR SOLICITUD
                solicitud.Estado = "Aprobada";
                solicitud.FechaProcesado = DateTime.Now;
                await _solicitudRepo.UpdateAsync(solicitud);

                // 4. EMAIL BIENVENIDA
                try
                {
                    string cuerpoEmail = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #456990; text-align: center;'>¡Felicitaciones, {solicitud.Nombre}!</h2>
                            <p>Tu solicitud ha sido <strong>APROBADA</strong>. Ya eres alumno regular de EduSys.</p>
                            <br>
                            <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #49BEAA;'>
                                <h3 style='margin-top: 0;'>Tus Credenciales de Acceso</h3>
                                <p><strong>Usuario:</strong> {solicitud.Email}</p>
                                <p><strong>Contraseña Inicial:</strong> {solicitud.Dni}</p>
                                <p><strong>Legajo:</strong> {nuevoLegajo}</p>
                            </div>
                            <div style='text-align: center; margin-top: 20px;'>
                                <a href='http://localhost:5000' style='background-color: #456990; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Ir al Campus</a>
                            </div>
                        </div>";

                    await _emailService.SendEmailAsync(solicitud.Email, "¡Bienvenido a EduSys! - Alta Exitosa", cuerpoEmail);
                }
                catch { }

                return Ok(new { message = "Alumno dado de alta exitosamente." });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, $"Error BD: {innerMessage}");
            }
        }

        // ... (El resto de métodos GetHistorial y DescargarZip se mantienen igual) ...
        // GET: api/inscripciones/historial
        [HttpGet("historial")]
        public async Task<ActionResult<List<SolicitudIngresoDTO>>> GetHistorial()
        {
            var lista = await _solicitudRepo.GetHistorialAsync();

            var listaDto = lista.Select(x => new SolicitudIngresoDTO
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Apellido = x.Apellido,
                Dni = x.Dni,
                Email = x.Email,
                FechaSolicitud = x.FechaSolicitud ?? DateTime.Now,
                FechaProcesado = x.FechaProcesado,
                Estado = x.Estado,
                NombreCarrera = x.IdCarreraInteresNavigation?.Nombre ?? "-",
                NombreSede = x.IdSedeNavigation?.Nombre ?? "-", // ✅ Ver Sede en historial
                ObservacionAdmin = x.ObservacionAdmin
            }).ToList();

            return Ok(listaDto);
        }
    }
}