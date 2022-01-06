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
            string? bucket = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
            string? path = Environment.GetEnvironmentVariable("FILE_PATH");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                if (!string.IsNullOrEmpty(bucket) && !string.IsNullOrEmpty(path))
                {
                    await _service.RunAsync(bucket, path, stoppingToken);
                } else
                {
                    throw new Exception("Bucket name and file path must be provided");
                }
            }
        }
    }
}