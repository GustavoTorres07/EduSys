namespace EduSys.Shared.Models;

public partial class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public int IdUsuario { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public bool? EsRevocado { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
