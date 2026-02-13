using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduSys.Shared.DTOs
{
    public class AlumnoDTO
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }

        // --- DATOS PERSONALES (Vienen de Usuario) ---
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? FotoPerfilUrl { get; set; }

        // --- DATOS ACADÉMICOS ---
        public string Legajo { get; set; } = string.Empty;
        public string EstadoAcademico { get; set; } = "Activo";
        public bool Activo { get; set; }

        // --- DATOS DE CARRERA Y PLAN ---
        public int IdPlanActual { get; set; }
        public string NombrePlan { get; set; } = string.Empty;

        public int IdCarrera { get; set; }
        public string NombreCarrera { get; set; } = string.Empty;

        // ✅ AGREGAR ESTOS DOS CAMPOS (Vital para filtrar la inscripción)
        public int IdSede { get; set; }
        public string NombreSede { get; set; } = string.Empty;
    }
}