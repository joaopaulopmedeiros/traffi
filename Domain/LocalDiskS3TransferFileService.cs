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
        /// Map files from dirs and send them to S3
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="filePath"></param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public async Task RunAsync
        (
            string bucketName, 
            string filePath, 
            CancellationToken stoppingToken
        )
        {
            try
            {
                if (File.Exists(filePath))
                {
                    await ProcessFileAsync(bucketName, filePath);
                }
                else if (Directory.Exists(filePath))
                {
                    await ProcessDirectoryAsync(bucketName, filePath);
                }
                else
                {
                    _logger.LogError("{0} is not a valid file or directory.", filePath);
                }
            } catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Send file to S3
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task ProcessFileAsync(string bucketName, string filePath)
        {   
            var client = new AmazonS3Client(Amazon.RegionEndpoint.SAEast1);

            try
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    FilePath = filePath,
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

        /// <summary>
        /// Check if error is caused by credentials
        /// </summary>
        /// <param name="amazonS3Exception"></param>
        /// <returns></returns>
        private bool isCredentialError(AmazonS3Exception amazonS3Exception)
        {
            return amazonS3Exception.ErrorCode != null &&
                  (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                  ||
                  amazonS3Exception.ErrorCode.Equals("InvalidSecurity"));
        }

        /// <summary>
        /// Map directories and get files to get processed
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task ProcessDirectoryAsync(string bucketName, string filePath)
        {
            string[] fileEntries = await Task.Run(() => Directory.GetFiles(filePath));

            foreach (string fileName in fileEntries)
            {
                await ProcessFileAsync(bucketName, fileName);
            }

            string[] subdirectoryEntries = await Task.Run(() => Directory.GetDirectories(filePath));
            
            foreach (string subdirectory in subdirectoryEntries)
            {
                await ProcessDirectoryAsync(bucketName, subdirectory);
            }
        }
    }
}
