using System;

namespace EduSys.Shared.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }

        // --- Datos de Identidad ---
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public DateOnly? FechaNacimiento { get; set; }
        public string? Sexo { get; set; }
        public string? LugarNacimiento { get; set; }
        public string? EstadoCivil { get; set; }

        // --- Datos de Contacto ---
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }

        // --- Contacto de Emergencia ---
        public string? NombreContactoEmergencia { get; set; }
        public string? TelefonoContactoEmergencia { get; set; }

        // --- Configuración y Estado ---
        public int IdRol { get; set; }
        public string? NombreRol { get; set; } // Opcional: Útil para mostrar en la interfaz sin hacer peticiones extra

        public int IdNacionalidad { get; set; }
        public string? NombreNacionalidad { get; set; } // Opcional: Útil para mostrar en combos o tablas

        public bool? Activo { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public bool DebeCambiarPass { get; set; }
    }
}