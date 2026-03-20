namespace EduSys.Shared.DTOs
{
    public class MesaFinalOfertaDTO
    {
        public int IdMesaFinal { get; set; }
        public int IdPlanMateria { get; set; }
        public string MateriaNombre { get; set; } = string.Empty;
        public int AnioCursada { get; set; }
        public DateTime FechaHora { get; set; }
        public string Tribunal { get; set; } = string.Empty; 
        public bool PuedeInscribirse { get; set; }
        public string MotivoBloqueo { get; set; } = string.Empty; 
        public string Condicion { get; set; } = string.Empty; 
        public bool YaInscripto { get; set; }
        public int? IdInscripcionFinal { get; set; }
    }
}
