namespace EduSys.Web.Utils
{
    public static class WidgetRegistry
    {
        public static readonly Dictionary<string, Type> Componentes = new()
        {
            { "AdminTickets", typeof(Pages.Widgets.WidgetAdminTickets) },
            { "AdminMetricas", typeof(Pages.Widgets.WidgetAdminMetricas) },
            { "AdminActividad", typeof(Pages.Widgets.WidgetAdminActividad) },

            { "Notificaciones", typeof(Pages.Widgets.WidgetNotificaciones) },

            { "DocenteAgenda", typeof(Pages.Widgets.WidgetDocenteAgenda) },
            { "DocenteAccesos", typeof(Pages.Widgets.WidgetDocenteAccesos) },
            
            // 🚀 NUEVOS WIDGETS DEL ALUMNO
            { "AlumnoAccesos", typeof(Pages.Widgets.WidgetAlumnoAccesos) },
            { "AlumnoAgendaHoy", typeof(Pages.Widgets.WidgetAlumnoAgendaHoy) },
            { "AlumnoExamenes", typeof(Pages.Widgets.WidgetAlumnoExamenes) }
        };
    }

    public class WidgetConfig
    {
        public string Id { get; set; } = string.Empty;
        public string ComponentKey { get; set; } = string.Empty;
        public int ColSpanXs { get; set; } = 12;
        public int ColSpanMd { get; set; } = 6;
        public int ColSpanLg { get; set; } = 4;
    }
}