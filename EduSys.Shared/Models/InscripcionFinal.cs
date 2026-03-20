namespace EduSys.Shared.Models;

public partial class InscripcionFinal
{
    public int Id { get; set; }
    public int IdAlumno { get; set; }
    public int IdMesaFinal { get; set; }
    public DateTime? FechaInscripcion { get; set; }
    public bool? Asistencia { get; set; }
    public decimal? Nota { get; set; }
    public string? Estado { get; set; }
    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;
    public virtual MesaFinal IdMesaFinalNavigation { get; set; } = null!;
}
