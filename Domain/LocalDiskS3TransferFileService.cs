using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace FileMigration.WorkerService.Domain
{
    public class LocalDiskS3TransferFileService : ITransferFileService
    {
        private readonly ILogger<LocalDiskS3TransferFileService> _logger;

        public LocalDiskS3TransferFileService
        (
            ILogger<LocalDiskS3TransferFileService> logger
        )
        {
            _logger = logger;
        }

        /// <summary>
        /// Mapeia arquivos em diretório e sub-diretórios do disco local e
        /// em sequência os envia para o S3
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public async Task RunAsync(CancellationToken stoppingToken)
        {
            try
            {
                string path = "C:\\example-files";

                if (File.Exists(path))
                {
                    await ProcessFileAsync(path);
                }
                else if (Directory.Exists(path))
                {
                    await ProcessDirectoryAsync(path);
                }
                else
                {
                    _logger.LogError("{0} is not a valid file or directory.", path);
                }
            } catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task ProcessFileAsync(string path)
        {
            Console.WriteLine("Processed file '{0}'.", path);
            
            var client = new AmazonS3Client(Amazon.RegionEndpoint.SAEast1);

            try
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = "s3-bpm-etpr-historico-exemplo-dev",
                    FilePath = path,
                    ContentType = "text/plain"
                };

                PutObjectResponse response = await client.PutObjectAsync(putRequest);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (isCredentialError(amazonS3Exception))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
        }

        private bool isCredentialError(AmazonS3Exception ex)
        {
            return ex.ErrorCode != null && 
                (
                    ex.ErrorCode.Equals("InvalidAccessKeyId") ||  
                    ex.ErrorCode.Equals("InvalidSecurity")
                );
        }

        private async Task ProcessDirectoryAsync(string path)
        {
            string[] fileEntries = await Task.Run(() => Directory.GetFiles(path));

            foreach (string fileName in fileEntries)
            {
                await ProcessFileAsync(fileName);
            }

            string[] subdirectoryEntries = await Task.Run(() => Directory.GetDirectories(path));
            
            foreach (string subdirectory in subdirectoryEntries)
            {
                await ProcessDirectoryAsync(subdirectory);
            }
        }
    }
}
