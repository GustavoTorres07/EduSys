using EduSys.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSys.Api.Data;

public partial class EduSysDbContext : DbContext
{
    public EduSysDbContext()
    {
    }

    public EduSysDbContext(DbContextOptions<EduSysDbContext> options)
        : base(options)
    {
    }

    // --- DBSETS ---
    public virtual DbSet<Alumno> Alumnos { get; set; }
    public virtual DbSet<Asistencia> Asistencia { get; set; }
    public virtual DbSet<Aula> Aulas { get; set; }
    public virtual DbSet<Carrera> Carreras { get; set; }
    public virtual DbSet<CarreraSede> CarreraSedes { get; set; }
    public virtual DbSet<Comision> Comisions { get; set; }
    public virtual DbSet<Correlatividad> Correlatividads { get; set; }
    public virtual DbSet<Docente> Docentes { get; set; }
    public virtual DbSet<DocenteComision> DocenteComisions { get; set; }
    public virtual DbSet<Evaluacion> Evaluacions { get; set; }
    public virtual DbSet<HorarioComision> HorarioComisions { get; set; }
    public virtual DbSet<InscripcionCursada> InscripcionCursada { get; set; }
    public virtual DbSet<InscripcionFinal> InscripcionFinals { get; set; }
    public virtual DbSet<Materia> Materia { get; set; }
    public virtual DbSet<MesaFinal> MesaFinals { get; set; }
    public virtual DbSet<Nacionalidad> Nacionalidads { get; set; }
    public virtual DbSet<Nota> Nota { get; set; }
    public virtual DbSet<PeriodoAcademico> PeriodoAcademicos { get; set; }
    public virtual DbSet<Permiso> Permisos { get; set; }
    public virtual DbSet<PlanEstudio> PlanEstudios { get; set; }
    public virtual DbSet<PlanMateria> PlanMateria { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Rol> Rols { get; set; }
    public virtual DbSet<Sede> Sedes { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<VentanaOperativa> VentanaOperativas { get; set; }
    public virtual DbSet<Regimen> Regimenes { get; set; }
    public virtual DbSet<Modalidad> Modalidads { get; set; }
    public virtual DbSet<CarreraModalidad> CarreraModalidads { get; set; }
    public virtual DbSet<PlanEstudioSede> PlanEstudioSedes { get; set; }
    public virtual DbSet<SolicitudIngreso> SolicitudIngresos { get; set; }
    public virtual DbSet<Notificacion> Notificacions { get; set; }
    public virtual DbSet<EstadoMateria> EstadoMaterias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alumno>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Alumno__3214EC07CC822989");

            entity.ToTable("Alumno");

            entity.HasIndex(e => e.Legajo, "UQ__Alumno__0E01039A7F04A3C6").IsUnique();
            entity.HasIndex(e => e.IdUsuario, "UQ__Alumno__5B65BF96DEC3DEA0").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.EstadoAcademico)
                .HasMaxLength(50)
                .HasDefaultValue("Activo");
            entity.Property(e => e.Legajo).HasMaxLength(50);

            // --- NUEVOS CAMPOS ALUMNO ---
            entity.Property(e => e.TituloSecundarioEntregado).HasDefaultValue(false);
            entity.Property(e => e.FechaIngreso).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Observaciones).HasColumnType("nvarchar(max)");
            // ----------------------------

            entity.HasOne(d => d.IdPlanActualNavigation).WithMany(p => p.Alumnos)
                .HasForeignKey(d => d.IdPlanActual)
                .HasConstraintName("FK_Alumno_Plan");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Alumno)
                .HasForeignKey<Alumno>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alumno_Usuario");

            entity.HasOne(d => d.IdSedeNavigation)
        .WithMany() // Una sede tiene muchos alumnos
        .HasForeignKey(d => d.IdSede)
        .OnDelete(DeleteBehavior.ClientSetNull)
        .HasConstraintName("FK_Alumno_Sede");
        });

        modelBuilder.Entity<Regimen>(entity =>
        {
            entity.ToTable("Regimen");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<PlanMateria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlanMate__3214EC0724FB791E");

            entity.Property(e => e.NotaMinimaRegularizar).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.NotaMinimaAprobacion).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.NotaMinimaPromocion).HasColumnType("decimal(4, 2)");

            entity.Property(e => e.EsPromocionable).HasDefaultValue(true);
            entity.Property(e => e.TieneFinalObligatorio).HasDefaultValue(false);
            entity.Property(e => e.TipoCalificacion).HasDefaultValue(0);
            entity.Property(e => e.CantidadParciales).HasDefaultValue(2);
            entity.Property(e => e.VigenciaCursadaAnios).HasDefaultValue(3);

            entity.HasOne(d => d.IdEstadoPromocionNavigation).WithMany().HasForeignKey(d => d.IdEstadoPromocion);
            entity.HasOne(d => d.IdEstadoRegularNavigation).WithMany().HasForeignKey(d => d.IdEstadoRegular);
            // ✅ AQUÍ AGREGAS TUS NUEVAS LÍNEAS:
            entity.HasOne(d => d.IdEstadoSiDesapruebaNavigation).WithMany().HasForeignKey(d => d.IdEstadoSiDesaprueba);
            entity.HasOne(d => d.IdEstadoSiFaltaAsistenciaNavigation).WithMany().HasForeignKey(d => d.IdEstadoSiFaltaAsistencia);
            entity.Property(e => e.MantienePromocionRecuperatorio).HasDefaultValue(false);

            entity.HasOne(d => d.IdMateriaNavigation).WithMany(p => p.PlanMateria)
                .HasForeignKey(d => d.IdMateria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanMateria_Materia");

            entity.HasOne(d => d.IdPlanNavigation).WithMany(p => p.PlanMateria)
                .HasForeignKey(d => d.IdPlan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanMateria_Plan");

            entity.HasOne(d => d.IdRegimenNavigation)
                  .WithMany()
                  .HasForeignKey(d => d.IdRegimen)
                  .HasConstraintName("FK_PlanMateria_Regimen");
        });

        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Asistenc__3214EC0775A00304");
            entity.Property(e => e.Observacion).HasMaxLength(200);

            entity.Property(e => e.EsJustificado).HasDefaultValue(false);
            entity.Property(e => e.UrlCertificado).HasMaxLength(500);
            // ---------------------------------

            entity.HasOne(d => d.IdInscripcionCursadaNavigation).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.IdInscripcionCursada)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asistencia_Inscripcion");
        });

        modelBuilder.Entity<Aula>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Aula__3214EC0789B514C9");
            entity.ToTable("Aula");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);

            entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.Aulas)
                .HasForeignKey(d => d.IdSede)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Aula_Sede");
        });

        modelBuilder.Entity<Carrera>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Carrera__3214EC074ADDBD03");
            entity.ToTable("Carrera");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Titulo).HasMaxLength(150);
        });

        modelBuilder.Entity<CarreraSede>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CarreraS__3214EC07229BF87A");
            entity.ToTable("CarreraSede");
            entity.Property(e => e.Activo).HasDefaultValue(true);

            entity.HasOne(d => d.IdCarreraNavigation).WithMany(p => p.CarreraSedes)
                .HasForeignKey(d => d.IdCarrera)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarreraSede_Carrera");

            entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.CarreraSedes)
                .HasForeignKey(d => d.IdSede)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarreraSede_Sede");
        });

        modelBuilder.Entity<Comision>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comision__3214EC0723F9A4BB");
            entity.ToTable("Comision");
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.CupoMaximo).HasDefaultValue(50);
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Abierta");
            entity.Property(e => e.Turno).HasMaxLength(20);

            entity.HasOne(d => d.IdPeriodoNavigation).WithMany(p => p.Comisions)
                .HasForeignKey(d => d.IdPeriodo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comision_Periodo");

            entity.HasOne(d => d.IdPlanMateriaNavigation).WithMany(p => p.Comisions)
                .HasForeignKey(d => d.IdPlanMateria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comision_Materia");

            entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.Comisions)
                .HasForeignKey(d => d.IdSede)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comision_Sede");
        });

        modelBuilder.Entity<Correlatividad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Correlat__3214EC07B7A6DEE5");
            entity.ToTable("Correlatividad");
            entity.Property(e => e.TipoRequisito).HasMaxLength(20);

            // ✅ ANTES: .WithMany()  ← EF creaba relaciones shadow duplicadas
            // ✅ AHORA: referenciar las colecciones reales del modelo PlanMateria
            entity.HasOne(d => d.IdPlanMateriaOrigenNavigation)
                .WithMany(p => p.CorrelativasComoOrigen)
                .HasForeignKey(d => d.IdPlanMateriaOrigen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Corr_Origen");

            entity.HasOne(d => d.IdPlanMateriaRequisitoNavigation)
                .WithMany(p => p.CorrelativasComoRequisito)
                .HasForeignKey(d => d.IdPlanMateriaRequisito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Corr_Requisito");
        });

        modelBuilder.Entity<Docente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Docente__3214EC0772D5D621");
            entity.ToTable("Docente");
            entity.HasIndex(e => e.Legajo, "UQ__Docente__0E01039ABBA6DA42").IsUnique();
            entity.HasIndex(e => e.IdUsuario, "UQ__Docente__5B65BF96EB37ABF1").IsUnique();
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Legajo).HasMaxLength(50);
            entity.Property(e => e.TituloAcademico).HasMaxLength(100);

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Docente)
                .HasForeignKey<Docente>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Docente_Usuario");
        });

        modelBuilder.Entity<DocenteComision>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DocenteC__3214EC07261A4027");
            entity.ToTable("DocenteComision");
            entity.Property(e => e.RolDocente).HasMaxLength(30);

            entity.HasOne(d => d.IdComisionNavigation).WithMany(p => p.DocenteComisions)
                .HasForeignKey(d => d.IdComision)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocCom_Comision");

            entity.HasOne(d => d.IdDocenteNavigation).WithMany(p => p.DocenteComisions)
                .HasForeignKey(d => d.IdDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocCom_Docente");
        });

        modelBuilder.Entity<Evaluacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Evaluaci__3214EC07C0022CF8");
            entity.ToTable("Evaluacion");
            entity.Property(e => e.EsRecuperatorio).HasDefaultValue(false);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Ponderacion).HasDefaultValue(0m).HasColumnType("decimal(5, 2)");

            entity.Property(e => e.EstadoActa).HasMaxLength(20).HasDefaultValue("Abierta");
            entity.Property(e => e.FechaCierre).HasColumnType("datetime");
            entity.Property(e => e.Libro).HasMaxLength(20);
            entity.Property(e => e.Folio).HasMaxLength(20);

            entity.Property(e => e.RequiereConfirmacion).HasDefaultValue(false);
            entity.Property(e => e.HorasAnticipacionConfirmar).HasDefaultValue(72);
            entity.Property(e => e.HorasAnticipacionBaja).HasDefaultValue(48);

            // ==============================================================
            // ✅ AQUÍ ESTÁ LA SOLUCIÓN: MAPEO DE LA RELACIÓN PADRE-HIJO
            // ==============================================================
            entity.HasOne(d => d.IdEvaluacionPadreNavigation)
                            .WithMany(e => e.EvaluacionesHijas)   // ← reemplazá el WithMany() vacío por esto
                            .HasForeignKey(d => d.IdEvaluacionPadre)
                            .HasConstraintName("FK_Evaluacion_Padre");

            entity.HasOne(d => d.IdComisionNavigation).WithMany(p => p.Evaluacions)
                .HasForeignKey(d => d.IdComision)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Evaluacion_Comision");
        });

        modelBuilder.Entity<HorarioComision>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HorarioC__3214EC07FACFF56A");
            entity.ToTable("HorarioComision");
            entity.Property(e => e.DiaSemana).HasMaxLength(15);

            entity.HasOne(d => d.IdComisionNavigation).WithMany(p => p.HorarioComisions)
                .HasForeignKey(d => d.IdComision)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Horario_Comision");

            entity.HasOne(d => d.IdAulaNavigation).WithMany().HasForeignKey(d => d.IdAula).HasConstraintName("FK_Horario_Aula");
        });

        modelBuilder.Entity<InscripcionCursada>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inscripc__3214EC078AE1CC7D");

            entity.HasIndex(e => new { e.IdAlumno, e.IdComision }, "UQ_Inscripcion_Alumno_Comision").IsUnique();

            entity.Property(e => e.CursadaCerrada).HasDefaultValueSql("((0))");
            entity.Property(e => e.CondicionFinal).HasMaxLength(20);
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.FechaInscripcion).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.NotaFinalCursada).HasColumnType("decimal(4, 2)");

            entity.HasOne(d => d.IdEstadoMateriaNavigation)
            .WithMany(p => p.InscripcionCursadas)
            .HasForeignKey(d => d.IdEstadoMateria)
            .HasConstraintName("FK_InsCursada_Estado");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.InscripcionCursada)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InsCursada_Alumno");

            entity.HasOne(d => d.IdComisionNavigation).WithMany(p => p.InscripcionCursada)
                .HasForeignKey(d => d.IdComision)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InsCursada_Comision");
        });

        modelBuilder.Entity<InscripcionFinal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inscripc__3214EC07B0E10B7E");
            entity.ToTable("InscripcionFinal");
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Inscripto");
            entity.Property(e => e.FechaInscripcion).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Nota).HasColumnType("decimal(4, 2)");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.InscripcionFinals)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InsFinal_Alumno");

            entity.HasOne(d => d.IdMesaFinalNavigation).WithMany(p => p.InscripcionFinals)
                .HasForeignKey(d => d.IdMesaFinal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InsFinal_Mesa");
        });

        modelBuilder.Entity<Materia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Materia__3214EC07671843A9");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(150);
        });

        modelBuilder.Entity<MesaFinal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MesaFina__3214EC074DFD1738");
            entity.ToTable("MesaFinal");
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Programada");
            entity.Property(e => e.FechaHora).HasColumnType("datetime");
            entity.Property(e => e.Folio).HasMaxLength(20);
            entity.Property(e => e.Libro).HasMaxLength(20);

            entity.HasOne(d => d.IdPeriodoNavigation).WithMany(p => p.MesaFinals)
                .HasForeignKey(d => d.IdPeriodo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mesa_Periodo");

            entity.HasOne(d => d.IdPlanMateriaNavigation).WithMany(p => p.MesaFinals)
                .HasForeignKey(d => d.IdPlanMateria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mesa_Materia");

            entity.HasOne(d => d.IdPresidenteMesaNavigation).WithMany(p => p.MesaFinals)
                .HasForeignKey(d => d.IdPresidenteMesa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mesa_Presidente");
        });

        modelBuilder.Entity<Nacionalidad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nacional__3214EC078FE7670A");
            entity.ToTable("Nacionalidad");
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("Notificacion");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Titulo).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Mensaje).IsRequired(); // nvarchar(max)
            entity.Property(e => e.Fecha).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Leida).HasDefaultValue(false);
            entity.Property(e => e.Tipo).HasMaxLength(50);

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificacion_Usuario");
        });

        modelBuilder.Entity<Nota>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nota__3214EC078CE7CF38");
            entity.Property(e => e.FechaCarga).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Observacion).HasMaxLength(200);
            entity.Property(e => e.Valor).HasColumnType("decimal(4, 2)");

            entity.HasOne(d => d.IdEvaluacionNavigation).WithMany(p => p.Nota)
                .HasForeignKey(d => d.IdEvaluacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Nota_Evaluacion");

            entity.HasOne(d => d.IdInscripcionCursadaNavigation).WithMany(p => p.Nota)
                .HasForeignKey(d => d.IdInscripcionCursada)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Nota_Inscripcion");
        });

        modelBuilder.Entity<PeriodoAcademico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PeriodoA__3214EC0772FBA47C");
            entity.ToTable("PeriodoAcademico");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Abierto");
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permiso__3214EC076E1CB54A");
            entity.ToTable("Permiso");
            entity.HasIndex(e => e.Codigo, "UQ__Permiso__06370DAC289C3EA6").IsUnique();
            entity.Property(e => e.Codigo).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.Modulo).HasMaxLength(50);
        });

        modelBuilder.Entity<PlanEstudio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlanEstu__3214EC07D097D635");
            entity.ToTable("PlanEstudio");
            entity.Property(e => e.EsVigente).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.ResolucionMinisterial).HasMaxLength(100);

            entity.HasOne(d => d.IdCarreraNavigation).WithMany(p => p.PlanEstudios)
                .HasForeignKey(d => d.IdCarrera)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Plan_Carrera");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC079E230024");
            entity.ToTable("RefreshToken");
            entity.Property(e => e.EsRevocado).HasDefaultValue(false);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.FechaExpiracion).HasColumnType("datetime");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshToken_Usuario");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rol__3214EC0712833D5A");
            entity.ToTable("Rol");
            entity.HasIndex(e => e.Nombre, "UQ__Rol__75E3EFCF49478A24").IsUnique();
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.Nombre).HasMaxLength(50);

            entity.HasMany(d => d.IdPermisos).WithMany(p => p.IdRols)
                .UsingEntity<Dictionary<string, object>>(
                    "RolPermiso",
                    r => r.HasOne<Permiso>().WithMany().HasForeignKey("IdPermiso").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_RolPermiso_Permiso"),
                    l => l.HasOne<Rol>().WithMany().HasForeignKey("IdRol").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_RolPermiso_Rol"),
                    j =>
                    {
                        j.HasKey("IdRol", "IdPermiso").HasName("PK__RolPermi__BA9F7EA0C12890AA");
                        j.ToTable("RolPermiso");
                    });
        });
        modelBuilder.Entity<PlanEstudioSede>(entity =>
        {
            entity.ToTable("PlanEstudioSede");
            entity.HasKey(e => new { e.IdPlan, e.IdSede });

            entity.HasOne(d => d.IdPlanNavigation)
                .WithMany(p => p.PlanEstudioSedes)
                .HasForeignKey(d => d.IdPlan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanSede_Plan");

            entity.HasOne(d => d.IdSedeNavigation)
                .WithMany(p => p.PlanEstudioSedes)
                .HasForeignKey(d => d.IdSede)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanSede_Sede");
        });
        modelBuilder.Entity<Sede>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sede__3214EC07274AAA10");
            entity.ToTable("Sede");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CodigoPostal).HasMaxLength(20);
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<EstadoMateria>(entity =>
        {
            entity.ToTable("EstadoMateria");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Activo).HasDefaultValueSql("((1))");
            entity.Property(e => e.EsAprobatoria).HasDefaultValueSql("((0))");
            entity.Property(e => e.HabilitaFinal).HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC075DC31621");
            entity.ToTable("Usuario");
            entity.HasIndex(e => e.Email, "UQ__Usuario__A9D105342ECC2688").IsUnique();
            entity.HasIndex(e => e.Dni, "UQ__Usuario__C035B8DDEF94EA3C").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Dni).HasMaxLength(20).HasColumnName("DNI");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(50);

            // --- NUEVOS CAMPOS USUARIO ---
            entity.Property(e => e.FotoPerfilUrl).HasMaxLength(255);
            entity.Property(e => e.NombreContactoEmergencia).HasMaxLength(150);
            entity.Property(e => e.TelefonoContactoEmergencia).HasMaxLength(50);
            // -----------------------------

            entity.HasOne(d => d.IdNacionalidadNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdNacionalidad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Nacionalidad");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });


        modelBuilder.Entity<VentanaOperativa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VentanaO__3214EC076D6B6E71");
            entity.ToTable("VentanaOperativa");
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.TipoAccion).HasMaxLength(50);

            entity.HasOne(d => d.IdPeriodoNavigation).WithMany(p => p.VentanaOperativas)
                .HasForeignKey(d => d.IdPeriodo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventana_Periodo");
            // Relación Carrera (Opcional)
            entity.HasOne(d => d.IdCarreraNavigation)
                      .WithMany()
                      .HasForeignKey(d => d.IdCarrera)
                      .HasConstraintName("FK_Ventana_Carrera");

            entity.HasOne(d => d.IdSedeNavigation)
                  .WithMany()
                  .HasForeignKey(d => d.IdSede)
                  .HasConstraintName("FK_Ventana_Sede");
        });

        modelBuilder.Entity<Modalidad>(entity =>
        {
            entity.ToTable("Modalidad");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Codigo).HasMaxLength(20);
            entity.Property(e => e.Activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<CarreraModalidad>(entity =>
        {
            entity.ToTable("CarreraModalidad");
            entity.HasKey(e => e.Id);

            entity.HasOne(d => d.IdCarreraNavigation).WithMany(p => p.CarreraModalidads)
                .HasForeignKey(d => d.IdCarrera)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarreraModalidad_Carrera");

            entity.HasOne(d => d.IdModalidadNavigation).WithMany(p => p.CarreraModalidads)
                .HasForeignKey(d => d.IdModalidad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarreraModalidad_Modalidad");
        });

        modelBuilder.Entity<SolicitudIngreso>(entity =>
        {
            entity.ToTable("SolicitudIngreso");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Apellido).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Dni).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Telefono).HasMaxLength(50);
            entity.Property(e => e.Direccion).HasMaxLength(200);

            // Rutas de archivos
            entity.Property(e => e.RutaFotoPerfil).HasMaxLength(500);
            entity.Property(e => e.RutaFotoDniFrente).HasMaxLength(500);
            entity.Property(e => e.RutaFotoDniDorso).HasMaxLength(500);
            entity.Property(e => e.RutaTituloSecundario).HasMaxLength(500);
            entity.Property(e => e.RutaAntecedentesPenales).HasMaxLength(500);
            entity.Property(e => e.RutaFotoSosteniendoDNI).HasMaxLength(500);

            entity.Property(e => e.Estado).HasMaxLength(50).HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaSolicitud).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ObservacionAdmin).HasColumnType("nvarchar(max)");
            entity.Property(e => e.FechaNacimiento).HasColumnType("date");

            entity.HasOne(d => d.IdCarreraInteresNavigation)
                  .WithMany()
                  .HasForeignKey(d => d.IdCarreraInteres)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_SolicitudIngreso_Carrera");

            entity.HasOne(d => d.IdSedeNavigation)
                  .WithMany()
                  .HasForeignKey(d => d.IdSede)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_SolicitudIngreso_Sede");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}