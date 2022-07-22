using ExtractorFunc.Models;
using ExtractorFunc.Repos;
using ExtractorFunc.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ExtractorFunc;

/// <summary>
/// A function that extracts docs.
/// </summary>
public class ExtractDocsFunction
{
    private const string TriggerContainerName = "wns-data-extract-trigger";

    private readonly IDataExtractRepo blobRepo;
    private readonly IClaimDocsRepo claimDocumentRepo;
    private readonly IRunConfigParser runConfigParser;

    /// <summary>
    /// Initialises a new instance of the <see cref="ExtractDocsFunction"/> class.
    /// </summary>
    /// <param name="blobRepo">The blob repo.</param>
    /// <param name="claimDocumentRepo">The claim document repo.</param>
    /// <param name="runConfigParser">The run config file parser.</param>
    public ExtractDocsFunction(
        IDataExtractRepo blobRepo,
        IClaimDocsRepo claimDocumentRepo,
        IRunConfigParser runConfigParser)
    {
        this.blobRepo = blobRepo;
        this.claimDocumentRepo = claimDocumentRepo;
        this.runConfigParser = runConfigParser;
    }

    /// <summary>
    /// Runs the function.
    /// </summary>
    /// <param name="triggerFile">The trigger payload.</param>
    /// <param name="triggerFileName">The trigger file name.</param>
    /// <param name="logger">The logger.</param>
    [FunctionName("ExtractDocs")]
    public async Task Run(
        [BlobTrigger($"{TriggerContainerName}/{{triggerFileName}}", Connection = "ExportBlobStorage")] Stream triggerFile,
        string triggerFileName,
        ILogger logger)
    {
        logger.LogInformation("Function start!!!");
        Console.WriteLine("INFO: Start!!!");

        var results = await RunInternalAsync(triggerFile, triggerFileName, logger);
        await blobRepo.UploadResultsAsync(results);
    }

    private async Task<RunResult> RunInternalAsync(Stream triggerFile, string triggerFileName, ILogger logger)
    {
        var retVal = new RunResult();
        try
        {
            var extension = Path.GetExtension(triggerFileName);
            retVal.RunConfig = runConfigParser.Parse(triggerFile, extension);
            var documents = await claimDocumentRepo.GetClaimDocumentsAsync(retVal.RunConfig);
            retVal.DocumentsFound = documents.Count;
            retVal.DocumentsFoundByClaimType = new()
            {
                { ClaimType.Continuation, documents.Count(d => d.ClaimType == ClaimType.Continuation) },
                { ClaimType.Claim, documents.Count(d => d.ClaimType == ClaimType.Claim) },
            };

            retVal.FailedUris = new List<string>();
            retVal.DocumentsExportedByClaimType = new()
            {
                { ClaimType.Continuation, 0 },
                { ClaimType.Claim, 0},
            };

            foreach (var document in documents)
            {
                try
                {
                    await blobRepo.CopyDocumentAsync(document);
                    Console.WriteLine("Copied blob " + document.BlobUri);
                    retVal.DocumentsExportedByClaimType[document.ClaimType]++;
                }
                catch (Exception ex)
                {
                    var message = "Failed to copy blob " + document.BlobUri;
                    Console.WriteLine("ERROR: " + message);
                    logger.LogError(ex, message);
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
