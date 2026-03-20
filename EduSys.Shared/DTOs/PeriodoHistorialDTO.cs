namespace EduSys.Shared.DTOs
{    public class PeriodoHistorialDTO
    {
        public int IdPeriodo { get; set; }
        public string NombrePeriodo { get; set; } = string.Empty;
        public int Anio { get; set; }

        public List<DetalleCursadaDTO> Materias { get; set; } = new();
    }
    public class DetalleCursadaDTO
    {
        public string Materia { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty; 
        public string Condicion { get; set; } = string.Empty;

        public decimal? NotaFinal { get; set; }
        public int PorcentajeAsistencia { get; set; }
        public List<string> NotasParciales { get; set; } = new();
        public bool EsPositivo => Estado == "Promocionado" || Estado == "Regular" || Estado == "Aprobada";
        public bool EsCursando => Estado == "Cursando";
    }
}