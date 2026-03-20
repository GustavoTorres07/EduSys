namespace EduSys.Shared.Models;

public partial class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string Dni { get; set; } = null!;
    public DateOnly? FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string Email { get; set; } = null!;
    public string ClaveHash { get; set; } = null!;
    public int IdRol { get; set; }
    public int IdNacionalidad { get; set; }
    public bool? Activo { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public string? Sexo { get; set; }
    public string? LugarNacimiento { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Localidad { get; set; }
    public string? FotoPerfilUrl { get; set; }
    public string? NombreContactoEmergencia { get; set; }
    public string? TelefonoContactoEmergencia { get; set; }
    public bool DebeCambiarPass { get; set; } = false;
    public virtual Nacionalidad IdNacionalidadNavigation { get; set; } = null!;
    public virtual Rol IdRolNavigation { get; set; } = null!;
    public virtual Alumno? Alumno { get; set; }
    public virtual Docente? Docente { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}