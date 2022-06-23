using Azure.Storage.Blobs;
using ExtractorFunc.Models;

namespace ExtractorFunc.Helpers;

internal static class DocumentHelper
{
    private const string BlobExportFormat = "practice-{0}/{1}/claim-{2}/{3}/{4}";

    public static BlobClient ToSourceBlob(this ClaimDocument document, BlobServiceClient source)
    {
        var uri = new Uri(document.BlobUri);
        var containerName = uri.Segments.Skip(1).First().Trim('/');
        var blobFilePath = string.Join(string.Empty, uri.Segments.Skip(2));
        var container = source.GetBlobContainerClient(containerName);
        return container.GetBlobClient(blobFilePath);
    }

    public static string ToTargetPath(this ClaimDocument document)
        => string.Format(
            BlobExportFormat,
            document.PracticeId,
            document.ClaimType,
            document.ClaimId,
            document.DocumentType,
            new Uri(document.BlobUri).Segments.Last());
}
