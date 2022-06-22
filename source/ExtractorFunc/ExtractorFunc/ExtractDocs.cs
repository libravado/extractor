using System;
using System.IO;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExtractorFunc
{
    public class ExtractDocs
    {
        private readonly BlobServiceClient sourceAccount;
        private readonly BlobContainerClient exportContainer;
        private readonly ILogger<ExtractDocs> logger;

        public ExtractDocs(IConfiguration config, IHostingEnvironment env, ILogger<ExtractDocs> logger)
        {
            this.logger = logger;

            if (env.IsDevelopment())
            {
                sourceAccount = new("UseDevelopmentStorage=true");
                exportContainer = new("UseDevelopmentStorage=true", config["ExportBlobContainerName"]);
            }
            else
            {
                sourceAccount = new(
                    new Uri($"https://{config["SourceDocsStorageAccountName"]}.blob.core.windows.net"),
                    new DefaultAzureCredential());
                exportContainer = new(
                    new Uri($"https://{config["ExportBlobStorageAccountName"]}.blob.core.windows.net/{config["ExportBlobContainerName"]}"),
                    new DefaultAzureCredential());
            }

            exportContainer.CreateIfNotExists();
        }

        [FunctionName("ExtractDocs")]
        public void Run(
            [BlobTrigger("trigger/{name}", Connection = "TriggerBlobStorage")]Stream myBlob)
        {
            logger.LogInformation($"C# Blob trigger function Processed blob\n Size: {myBlob.Length} Bytes");
        }
    }
}
