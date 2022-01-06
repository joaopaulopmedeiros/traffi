using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMigration.WorkerService.Domain
{
    public class SharepointS3TransferFileService : ITransferFileService
    {
        public async Task RunAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
