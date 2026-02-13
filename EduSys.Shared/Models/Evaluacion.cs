namespace EduSys.Shared.Models;

public partial class Evaluacion
{
    public int Id { get; set; }
    public int IdComision { get; set; }
    public string Nombre { get; set; } = null!;
    public DateOnly Fecha { get; set; }
    public bool? EsRecuperatorio { get; set; }
    public decimal? Ponderacion { get; set; }

    // ✅ NUEVOS CAMPOS PARA ACTAS Y CONFIRMACIÓN
    public string EstadoActa { get; set; } = "Abierta";
    public DateTime? FechaCierre { get; set; }
    public string? Libro { get; set; }
    public string? Folio { get; set; }

    public bool? RequiereConfirmacion { get; set; }
    public int? HorasAnticipacionConfirmar { get; set; }
    public int? HorasAnticipacionBaja { get; set; }

    // Relaciones
    public virtual Comision IdComisionNavigation { get; set; } = null!;
    public virtual ICollection<Nota> Nota { get; set; } = new List<Nota>();
}
