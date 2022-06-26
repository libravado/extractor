using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ExtractorFunc.Models;
using ExtractorFunc.Services;

namespace ExtractorFunc.Repos;

/// <inheritdoc cref="IDataExtractRepo"/>
public class DataExtractRepo : IDataExtractRepo
{
    private const string TargetPathFormat = "practice-{0}/{1}/claim-{2}/{3}/{4}";

    private readonly BlobServiceClient sourceAccount;
    private readonly BlobContainerClient exportContainer;
    private readonly IBlobClientService blobClientService;

    /// <summary>
    /// Initialises a new instance of the <see cref="DataExtractRepo"/> class.
    /// </summary>
    /// <param name="env"></param>
    /// <param name="config"></param>
    /// <param name="blobClientService"></param>
    public DataExtractRepo(
        IHostingEnvironment env,
        IConfiguration config,
        IBlobClientService blobClientService)
    {
        this.blobClientService = blobClientService;

        sourceAccount = this.blobClientService.GetAccount(
            env.IsDevelopment() ? null : config["SourceDocsStorageAccountName"]);

        exportContainer = this.blobClientService.GetContainer(
            config["ExportBlobContainerName"],
            env.IsDevelopment() ? null : config["ExportBlobStorageAccountName"]);

        this.blobClientService.CreateIfNotExists(exportContainer);
    }

    /// <inheritdoc/>
    public string BuildDocumentPath(ClaimDocument document) => string.Format(
        TargetPathFormat,
        document.PracticeId,
        document.ClaimType,
        document.ClaimId,
        document.DocumentType,
        new Uri(document.BlobUri).Segments.Last());

    /// <inheritdoc/>
    public async Task CopyDocumentAsync(ClaimDocument document)
    {
        var source = blobClientService.GetBlobClient(sourceAccount, document.BlobUri);
        var exportPath = BuildDocumentPath(document);
        var target = exportContainer.GetBlobClient(exportPath);
        await blobClientService.CopyBlobAsync(source, target);
    }

    /// <inheritdoc/>
    public async Task UploadResultsAsync(RunResult results)
    {
        var fileName = $"runs/{results.LocalTimestamp:o}.results.json";
        await blobClientService.UploadJsonAsync(exportContainer, results, fileName);
    }
}
