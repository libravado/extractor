using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ExtractorFunc.Helpers;
using ExtractorFunc.Models;

namespace ExtractorFunc;

/// <summary>
/// A function that extracts docs.
/// </summary>
public class ExtractDocsFunction
{
    private const string TriggerContainerName = "wns-data-extract-trigger";

    private readonly string sourceDbConnection;
    private readonly BlobServiceClient sourceAccount;
    private readonly BlobContainerClient exportContainer;

    /// <summary>
    /// Initialises a new instance of the <see cref="ExtractDocsFunction"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="env">The hosting environment.</param>
    public ExtractDocsFunction(IConfiguration config, IHostingEnvironment env)
    {
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
        sourceDbConnection = config.GetConnectionString("SourceDb");
    }

    /// <summary>
    /// Runs the function.
    /// </summary>
    /// <param name="triggerFile">The trigger payload.</param>
    /// <param name="triggerFileName">The trigger file name.</param>
    [FunctionName("ExtractDocs")]
    public async Task Run(
        [BlobTrigger($"{TriggerContainerName}/{{triggerFileName}}", Connection = "TriggerBlobStorage")] Stream triggerFile,
        string triggerFileName)
    {
        var results = await RunInternalAsync(triggerFile, triggerFileName);
        await exportContainer.SendJsonAsync(results, $"runs/{results.LocalTimestamp:o}.results.json");
    }

    private async Task<RunResult> RunInternalAsync(Stream triggerFile, string triggerFileName)
    {
        var retVal = new RunResult();

        try
        {
            retVal.RunConfig = TriggerParser.ReadTriggerConfig(triggerFile, triggerFileName);
            var documents = SqlHelper.GatherDocumentData(sourceDbConnection, retVal.RunConfig);
            retVal.DocumentsFound = documents.Count;
            retVal.FailedUris = new List<string>();
            foreach (var document in documents)
            {
                try
                {
                    var source = document.ToSourceBlob(sourceAccount);
                    var target = exportContainer.GetBlobClient(document.ToTargetPath());
                    await source.CopyToAsync(target);
                }
                catch
                {
                    retVal.FailedUris.Add(document.BlobUri);
                }
            }

            retVal.RanToCompletion = true;
        }
        catch (Exception ex)
        {
            retVal.ErrorMessage = $"{ex.GetType().Name}: {ex.Message}";
        }

        return retVal;
    }
}
