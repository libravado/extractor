using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pawtal.ExtractDocs.Func.Helpers;
using Pawtal.ExtractDocs.Func.Models;
using Pawtal.ExtractDocs.Func.Repos;
using Pawtal.ExtractDocs.Func.Services;

namespace Pawtal.ExtractDocs.Func;

/// <summary>
/// A function that extracts docs.
/// </summary>
public class ExtractDocsFunction
{
    private const string TriggerContainerName = "wns-data-extract-trigger";

    private readonly BlobServiceClient sourceAccount;
    private readonly BlobContainerClient exportContainer;
    private readonly IClaimDocumentRepo claimDocumentRepo;
    private readonly IBlobClientService blobClientService;
    private readonly IRunConfigParser runConfigParser;

    /// <summary>
    /// Initialises a new instance of the <see cref="ExtractDocsFunction"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="env">The hosting environment.</param>
    /// <param name="blobClientService">The blob client service.</param>
    /// <param name="claimDocumentRepo">The claim document repo.</param>
    /// <param name="runConfigParser">The run config file parser.</param>
    public ExtractDocsFunction(
        IConfiguration config,
        IHostingEnvironment env,
        IBlobClientService blobClientService,
        IClaimDocumentRepo claimDocumentRepo,
        IRunConfigParser runConfigParser)
    {
        this.blobClientService = blobClientService;
        this.claimDocumentRepo = claimDocumentRepo;
        this.runConfigParser = runConfigParser;

        sourceAccount = env.GetSourceBlobAccount(config);
        exportContainer = env.GetExportContainer(config);
        blobClientService.CreateIfNotExists(exportContainer);
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
        var fileName = $"runs/{results.LocalTimestamp:o}.results.json";
        await blobClientService.UploadJsonAsync(exportContainer, results, fileName);
    }

    private async Task<RunResult> RunInternalAsync(Stream triggerFile, string triggerFileName)
    {
        var retVal = new RunResult();

        try
        {
            var extension = Path.GetExtension(triggerFileName);
            retVal.RunConfig = runConfigParser.Parse(triggerFile, extension);

            var documents = claimDocumentRepo.GetClaimDocuments(retVal.RunConfig);
            retVal.DocumentsFound = documents.Count;
            retVal.FailedUris = new List<string>();
            foreach (var document in documents)
            {
                try
                {
                    var source = blobClientService.GetBlobClient(sourceAccount, document.BlobUri);
                    var target = exportContainer.GetBlobClient(document.ExportPath);
                    await blobClientService.CopyBlobAsync(source, target);
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
