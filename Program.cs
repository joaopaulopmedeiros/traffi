using FileMigration.WorkerService;
using FileMigration.WorkerService.Domain;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<ITransferFileService, LocalDiskS3TransferFileService>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
