using FileMigration.WorkerService.Domain;

namespace FileMigration.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITransferFileService _service;

        public Worker(ILogger<Worker> logger, ITransferFileService service)
        {
            _logger = logger;
            _service = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await _service.RunAsync(stoppingToken);
            }
        }
    }
}