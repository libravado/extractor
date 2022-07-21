using ExtractorFunc.Models;

namespace ExtractorFunc.Repos;

/// <summary>
/// Repository for claim document metadata.
/// </summary>
public interface IClaimDocsRepo
{
    /// <summary>
    /// Gets a list of claim documents from source, based on run configuration.
    /// </summary>
    /// <param name="runConfig"></param>
    /// <returns>A list of claim documents.</returns>
    public Task<List<ClaimDocument>> GetClaimDocumentsAsync(RunConfig runConfig);
}
