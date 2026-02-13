namespace EduSys.Shared.Models;

public partial class PeriodoAcademico
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public string? Estado { get; set; }

    public bool? Activo { get; set; }

    // --- ESTA ES LA LÍNEA QUE SOLUCIONA EL ERROR 500 ---
    // Sin esto, EF Core falla al intentar mapear la relación desde Comision
    public virtual ICollection<Comision> Comisions { get; set; } = new List<Comision>();
    // ---------------------------------------------------

    public virtual ICollection<MesaFinal> MesaFinals { get; set; } = new List<MesaFinal>();

    public virtual ICollection<VentanaOperativa> VentanaOperativas { get; set; } = new List<VentanaOperativa>();
}