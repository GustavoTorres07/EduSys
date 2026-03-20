namespace EduSys.Shared.Models;

public partial class Docente
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Legajo { get; set; } = null!;
    public string? TituloAcademico { get; set; }
    public bool? Activo { get; set; }
    public virtual ICollection<DocenteComision> DocenteComisions { get; set; } = new List<DocenteComision>();
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
    public virtual ICollection<MesaFinal> MesaFinals { get; set; } = new List<MesaFinal>();
}
