using Microsoft.EntityFrameworkCore;
using ExtractorFunc.Models;
using ExtractorFunc.Persistence;

namespace ExtractorFunc.Repos;

/// <inheritdoc cref="IClaimDocsRepo"/>
public class ClaimDocsEfRepo : IClaimDocsRepo
{
    private static readonly List<int> ClaimTypes = new(new[] { (int)ClaimType.Claim, (int)ClaimType.Continuation });

    private readonly SourceDbContext sourceDb;

    /// <summary>
    /// Initialises a new instance of the <see cref="ClaimDocsEfRepo"/> class.
    /// </summary>
    /// <param name="sourceDb">The source db.</param>
    public ClaimDocsEfRepo(SourceDbContext sourceDb)
    {
        this.sourceDb = sourceDb;
    }

    public async Task<List<ClaimDocument>> GetClaimDocumentsAync(RunConfig runConfig)
    {
        return new List<ClaimDocument>
        {
            new ClaimDocument(1, 1001, ClaimType.Claim, DocumentType.Invoice, "https://devstgsharedweu.blob.core.windows.net/source-docs/word-doc.docx"),
            new ClaimDocument(1, 1002, ClaimType.Continuation, DocumentType.Note, "https://devstgsharedweu.blob.core.windows.net/source-docs/1.pdf"),
        };
    }

    ///// <inheritdoc/>
    //public async Task<List<ClaimDocument>> GetClaimDocumentsAync(RunConfig runConfig)
    //    => await sourceDb.ClaimUploads
    //        .Include(cu => cu.Claim!.PracticePolicy)
    //        .Where(cu => runConfig.PracticeIds == null || runConfig.PracticeIds.Contains(cu.Claim!.PracticePolicy.PracticeId))
    //        .Where(cu => cu.Claim!.CreatedAt >= runConfig.ClaimsCreatedFrom && cu.Claim!.CreatedAt < runConfig.ClaimsCreatedTo)
    //        .Where(cu => ClaimTypes.Contains(cu.Claim!.TypeId))
    //        .Select(cu => new ClaimDocument(
    //            cu.Claim!.PracticePolicy.PracticeId,
    //            cu.Claim.Id,
    //            (ClaimType)cu.Claim.TypeId,
    //            (DocumentType)cu.TypeId,
    //            cu.BlobUri))
    //        .ToListAsync();
}
