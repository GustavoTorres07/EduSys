namespace EduSys.Shared.DTOs
{
    public class PlanillaNotasDTO
    {
        public int IdComision { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public string ComisionCodigo { get; set; } = string.Empty;

        public List<EvaluacionDTO> Evaluaciones { get; set; } = new();
        public List<NotaAlumnoDTO> Alumnos { get; set; } = new();
    }
}