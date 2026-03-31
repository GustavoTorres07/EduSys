using EduSys.Api.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EduSys.Api.Workers
{
    public class NotificacionesWorker : BackgroundService
    {
        private readonly ILogger<NotificacionesWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public NotificacionesWorker(ILogger<NotificacionesWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Trabajador de Notificaciones iniciado.");

            // El ciclo de vida del trabajador
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("⏰ Ejecutando revisión automática de notificaciones a las: {Time}", DateTimeOffset.Now);

                try
                {
                    // Necesitamos crear un "Scope" (ámbito) porque NotificationService y el DbContext 
                    // viven por petición HTTP, pero este Worker vive para siempre.
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var notificacionService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        // 1. Ejecutar la revisión de vencimientos
                        await notificacionService.GenerarAlertasVencimientoMesasAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico durante la ejecución del trabajador de notificaciones.");
                }

                // ⏳ ¿Cada cuánto queremos que revise? 
                // Para producción: TimeSpan.FromHours(24) (Una vez al día)
                // Para pruebas ahora mismo: TimeSpan.FromMinutes(5) (Cada 5 minutos)
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}