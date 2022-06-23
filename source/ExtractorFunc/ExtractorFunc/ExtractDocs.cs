using Azure.Storage.Blobs;
using ExtractorFunc.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExtractorFunc
{
    /// <summary>
    /// Extracts source documents into a separate container, based on filter parameters
    /// that are delivered within the contents of a blob trigger file.
    /// </summary>
    public class ExtractDocs
    {
        private const string TriggerContainerName = "wns-data-extract-trigger";

        private readonly string sourceDbConnection;
        private readonly BlobServiceClient sourceAccount;
        private readonly BlobContainerClient exportContainer;
        private readonly DateTime localTimestamp;
        private readonly string logPrefix;

        /// <summary>
        /// Initialises a new instance of the <see cref="ExtractDocs"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="env">The environment.</param>
        public ExtractDocs(IConfiguration config, IHostingEnvironment env)
        {
            localTimestamp = DateTime.Now;
            logPrefix = $"Extract {localTimestamp.Ticks}";
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

        /// <summary>
        /// Executes the function.
        /// </summary>
        /// <param name="triggerFile">The trigger file.</param>
        /// <param name="triggerFileName">The trigger file name.</param>
        /// <param name="logger">The logger.</param>
        [FunctionName("ExtractDocs")]
        public async Task Run(
            [BlobTrigger($"{TriggerContainerName}/{{triggerFileName}}", Connection = "TriggerBlobStorage")] Stream triggerFile,
            string triggerFileName,
            ILogger logger)
        {
            logger.LogInformation($"{logPrefix}: START");

            var runConfig = TriggerParser.ReadTriggerConfig(triggerFile, triggerFileName);
            var documents = SqlHelper.GatherDocumentData(sourceDbConnection, runConfig);
            var documentCount = documents.Count;

            logger.LogInformation($"{logPrefix}: Source documents found: {documentCount}");

            using var queryDataStream = RunDataHelper.PrepareRunDataJson(localTimestamp, runConfig, new { documentCount, documents });
            await exportContainer.UploadBlobAsync($"runs/{localTimestamp.Ticks}/querydata.json", queryDataStream);

            var failedUris = new List<string>();
            foreach (var document in documents)
            {
                try
                {
                    var source = document.ToSourceBlob(sourceAccount);
                    var target = exportContainer.GetBlobClient(document.ToTargetPath());
                    await BlobClientHelper.CopyBlobAsync(source, target);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"{logPrefix}: Failed to copy: {document.BlobUri}");
                    failedUris.Add(document.BlobUri);
                }
            }

            var failedUriCount = failedUris.Count;
            using var resultsStream = RunDataHelper.PrepareRunDataJson(localTimestamp, runConfig, new { documentCount, failedUriCount, failedUris });
            await exportContainer.UploadBlobAsync($"runs/{localTimestamp.Ticks}/results.json", resultsStream);

            logger.LogInformation($"{logPrefix}: END");
        }
    }
}
