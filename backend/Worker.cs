using backend.Presentation;

namespace backend
{
    /// <summary>
    /// Background service that hosts the TCP server for client connections.
    /// Manages the lifecycle of the TCP server and ensures graceful shutdown.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TcpServer _tcpServer;

        public Worker(ILogger<Worker> logger, TcpServer tcpServer)
        {
            _logger = logger;
            _tcpServer = tcpServer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker starting TCP Server...");

            try
            {
                await _tcpServer.StartAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Worker while running TCP Server.");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker stopping TCP Server...");
            await _tcpServer.StopAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}
