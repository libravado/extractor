using ExtractorFunc.Models;
using ExtractorFunc.Repos;
using ExtractorFunc.Services;
using Microsoft.Azure.WebJobs;

namespace ExtractorFunc;

/// <summary>
/// A function that extracts docs.
/// </summary>
public class ExtractDocsFunction
{
    private const string TriggerContainerName = "wns-data-extract-trigger";

    private readonly IBlobRepo blobRepo;
    private readonly IClaimDocsRepo claimDocumentRepo;
    private readonly IRunConfigParser runConfigParser;

    /// <summary>
    /// Initialises a new instance of the <see cref="ExtractDocsFunction"/> class.
    /// </summary>
    /// <param name="blobRepo">The blob repo.</param>
    /// <param name="claimDocumentRepo">The claim document repo.</param>
    /// <param name="runConfigParser">The run config file parser.</param>
    public ExtractDocsFunction(
        IBlobRepo blobRepo,
        IClaimDocsRepo claimDocumentRepo,
        IRunConfigParser runConfigParser)
    {
        this.blobRepo = blobRepo;
        this.claimDocumentRepo = claimDocumentRepo;
        this.runConfigParser = runConfigParser;

    /*
        //TODO!
            - The "pure" Blob library functions can then be moved from IBlobServiceClient into extensions (?? perhaps.. pending testability)
                - OR.... Make moar interfaces outta everything?
            - ALSO: Overall "Split" information regarding Claims vs PAs eg 70:30 (?) etc
            - ALSO: Unit tests, obvs
    */
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
        await blobRepo.UploadResultsAsync(results);
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
                    await blobRepo.CopyDocumentAsync(document);
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
