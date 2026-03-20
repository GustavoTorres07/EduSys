namespace EduSys.Shared.Models;

public partial class InscripcionCursada
{
    public int Id { get; set; }
    public int IdAlumno { get; set; }
    public int IdComision { get; set; }
    public DateTime? FechaInscripcion { get; set; }
    public string Estado { get; set; } = null!;
    public string? CondicionFinal { get; set; }
    public decimal? NotaFinalCursada { get; set; }
    public bool EsLibre { get; set; } = false;   
    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();
    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;
    public virtual Comision IdComisionNavigation { get; set; } = null!;
    public virtual ICollection<Nota> Nota { get; set; } = new List<Nota>();
    public int? IdEstadoMateria { get; set; }
    public bool CursadaCerrada { get; set; }
    public virtual EstadoMateria? IdEstadoMateriaNavigation { get; set; }
}
