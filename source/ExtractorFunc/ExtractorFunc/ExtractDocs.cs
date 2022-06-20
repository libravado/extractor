using System.IO;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ExtractorFunc
{
    public class ExtractDocs
    {
        private const string SourceStorageConnectionKey = "SourceStorageAccount";
        private const string TargetStorageConnectionKey = "TargetStorageAccount";
        private const string TriggerStorageConnectionKey = "TriggerStorageAccount";
        private const string SourceDocsContainerNameKey = "SourceDocsContainerName";
        private const string TargetBlobContainerNameKey = "TargetBlobContainerName";
        private const string TriggerContainerName = "trigger";

        private readonly BlobContainerClient docsContainer;
        private readonly BlobContainerClient exportContainer;

        public ExtractDocs(IConfiguration config)
        {
            var sourceConnection = config.GetConnectionString(SourceStorageConnectionKey);
            var targetConnection = config.GetConnectionString(TargetStorageConnectionKey);

            docsContainer = new(sourceConnection, config[SourceDocsContainerNameKey]);
            exportContainer = new(targetConnection, config[TargetBlobContainerNameKey]);

            docsContainer.CreateIfNotExists();
            exportContainer.CreateIfNotExists();
        }

        [FunctionName("ExtractDocs")]
        public void Run(
            [BlobTrigger($"{TriggerContainerName}/{{name}}", Connection = TriggerStorageConnectionKey)]Stream myBlob,
            ILogger logger)
        {
            logger.LogInformation($"C# Blob trigger function Processed blob\n Size: {myBlob.Length} Bytes");
        }
    }
}
