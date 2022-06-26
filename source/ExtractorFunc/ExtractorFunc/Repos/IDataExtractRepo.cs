using ExtractorFunc.Models;

namespace ExtractorFunc.Repos;

/// <summary>
/// Repository for managing document blobs.
/// </summary>
public interface IDataExtractRepo
{
    /// <summary>
    /// Moves a copy of the document to the target.
    /// </summary>
    /// <param name="document">The document.</param>
    public Task CopyDocumentAsync(ClaimDocument document);

    /// <summary>
    /// Constructs a document export file path.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <returns>The export path.</returns>
    public string BuildDocumentPath(ClaimDocument document);

    /// <summary>
    /// Uploads run results to the document export target.
    /// </summary>
    /// <param name="results">The results.</param>
    public Task UploadResultsAsync(RunResult results);
}
