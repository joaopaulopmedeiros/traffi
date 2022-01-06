﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMigration.WorkerService.Domain
{
    public interface ITransferFileService
    {
        public Task RunAsync(CancellationToken stoppingToken);
    }
}
