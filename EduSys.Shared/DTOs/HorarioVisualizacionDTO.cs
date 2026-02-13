namespace EduSys.Shared.DTOs
{
    public class HorarioVisualizacionDTO
    {
        public int Id { get; set; }

        // ✅ AGREGAR: Necesario para identificar la comisión única
        public int IdComision { get; set; }

        public string Materia { get; set; } = null!;
        public int AnioCursada { get; set; }

        // ✅ AGREGAR: Esta es la etiqueta que usa la tabla en la columna izquierda (Ej: "1º A")
        public string Curso { get; set; } = null!;

        public string CarreraNombre { get; set; } = null!;

        public string ComisionCodigo { get; set; } = null!;
        public string Dia { get; set; } = null!;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Aula { get; set; } = null!;
        public string Sede { get; set; } = null!;
    }
}
