using Azure.Identity;
using Azure.Storage.Blobs;
using ExtractorFunc.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExtractorFunc
{
    public class ExtractDocs
    {
        private const string ExportBlobFormat = "practice-{0}/{1}/claim-{2}/{3}/{4}";
        private const string TriggerContainerName = "wns-data-extract-trigger";

        private readonly string sourceDbConnection;
        private readonly BlobServiceClient sourceAccount;
        private readonly BlobContainerClient exportContainer;
        private readonly DateTime localTimestamp;
        private readonly string logPrefix;

        public ExtractDocs(IConfiguration config, IHostingEnvironment env)
        {
            localTimestamp = DateTime.Now;
            logPrefix = $"Extract {localTimestamp.Ticks}: ";
            sourceDbConnection = config.GetConnectionString("SourceDb");

            if (env.IsDevelopment())
            {
                sourceAccount = BlobClientHelper.GetDevAccount();
                exportContainer = BlobClientHelper.GetDevContainer(config["ExportBlobContainerName"]);
            }
            else
            {
                sourceAccount = BlobClientHelper.GetHostedAccount(config["SourceDocsStorageAccountName"]);
                exportContainer = BlobClientHelper.GetHostedContainer(
                    config["ExportBlobStorageAccountName"],
                    config["ExportBlobContainerName"]);
            }

            exportContainer.CreateIfNotExists();
        }

        [FunctionName("ExtractDocs")]
        public void Run(
            [BlobTrigger($"{TriggerContainerName}/{{name}}", Connection = "TriggerBlobStorage")] Stream myBlob,
            ILogger logger)
        {
            logger.LogInformation($"C# Blob trigger function Processed blob\n Size: {myBlob.Length} Bytes");
        }
    }
}
