using Pawtal.ExtractDocs.Func.Models;

namespace Pawtal.ExtractDocs.Func.Repos;

/// <summary>
/// Repository for claim document metadata.
/// </summary>
public interface IClaimDocumentRepo
{
    /// <summary>
    /// Gets a list of claim documents from source, based on run configuration.
    /// </summary>
    /// <param name="runConfig"></param>
    /// <returns>A list of claim documents.</returns>
    public List<ClaimDocument> GetClaimDocuments(RunConfig runConfig);
}
