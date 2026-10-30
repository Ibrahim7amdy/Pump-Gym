
using Microsoft.Extensions.Logging;
using Pump.Application.Interfaces.Sessions;

namespace Pump.Infrastructure.Jobs
{
    public class StaleSessionCleanupJob
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<StaleSessionCleanupJob> _logger;

        public StaleSessionCleanupJob(ISessionService sessionService, ILogger<StaleSessionCleanupJob> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            try
            {
                var closedCount = await _sessionService.CloseStaleSessionsAsync(ct);

                _logger.LogInformation("StaleSessionCleanupJob completed. Closed {Count} stale sessions.", closedCount);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogInformation("StaleSessionCleanupJob was cancelled.");

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"StaleSessionCleanupJob failed unexpectedly.");

                throw;
            }
        }
    }
}
