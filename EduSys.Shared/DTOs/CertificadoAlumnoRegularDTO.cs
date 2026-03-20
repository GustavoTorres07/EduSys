namespace EduSys.Shared.DTOs
{
    public class CertificadoAlumnoRegularDTO
    {
        public string InstitucionNombre { get; set; } = "EduSys Instituto Superior";
        public string Departamento { get; set; } = "DEPARTAMENTO DE ALUMNOS";
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public string PeriodoAcademico { get; set; } = string.Empty;
        public string AlumnoNombre { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
        public string RectorNombre { get; set; } = "Lic. Autoridad Académica";
        public string RectorCargo { get; set; } = "Secretario/a Académico/a";
        public string Ciudad { get; set; } = "Santa Rosa";
        public string Provincia { get; set; } = "La Pampa";
    }
}
