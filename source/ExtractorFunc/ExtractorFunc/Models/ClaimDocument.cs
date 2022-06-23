namespace ExtractorFunc.Models;

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
    string BlobUri);
