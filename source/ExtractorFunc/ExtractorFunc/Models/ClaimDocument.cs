namespace Pawtal.ExtractDocs.Func.Models;

/// <summary>
/// A claim document.
/// </summary>
/// <param name="PracticeId">The practice id.</param>
/// <param name="ClaimId">The claim id.</param>
/// <param name="ClaimType">The claim type.</param>
/// <param name="DocumentType">The document type.</param>
/// <param name="BlobUri">The document blob uri.</param>
public record ClaimDocument(
    int PracticeId,
    int ClaimId,
    ClaimType ClaimType,
    DocumentType DocumentType,
    string BlobUri)
{
    private const string TargetPathFormat = "practice-{0}/{1}/claim-{2}/{3}/{4}";

    /// <summary>
    /// Gets the target path for export.
    /// </summary>
    public string ExportPath => string.Format(
        TargetPathFormat,
        PracticeId,
        ClaimType,
        ClaimId,
        DocumentType,
        new Uri(BlobUri).Segments.Last());
}
