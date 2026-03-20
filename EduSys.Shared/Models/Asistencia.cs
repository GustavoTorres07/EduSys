namespace EduSys.Shared.Models;

public partial class Asistencia
{
    public int Id { get; set; }
    public int IdInscripcionCursada { get; set; }
    public DateOnly Fecha { get; set; }
    public bool EstaPresente { get; set; }
    public string? Observacion { get; set; }
    public virtual InscripcionCursada IdInscripcionCursadaNavigation { get; set; } = null!;
}
