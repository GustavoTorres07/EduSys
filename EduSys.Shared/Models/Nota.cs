namespace EduSys.Shared.Models;

public partial class Nota
{
    public int Id { get; set; }

    public int IdEvaluacion { get; set; }

    public int IdInscripcionCursada { get; set; }

    public decimal Valor { get; set; }

    public DateTime? FechaCarga { get; set; }

    public string? Observacion { get; set; }

    public virtual Evaluacion IdEvaluacionNavigation { get; set; } = null!;

    public virtual InscripcionCursada IdInscripcionCursadaNavigation { get; set; } = null!;
}
