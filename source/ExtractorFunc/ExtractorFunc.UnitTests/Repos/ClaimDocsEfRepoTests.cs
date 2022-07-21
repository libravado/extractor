using Microsoft.EntityFrameworkCore;
using ExtractorFunc.Models;
using ExtractorFunc.Persistence;
using ExtractorFunc.Persistence.Models;
using ExtractorFunc.Repos;

namespace ExtractorFunc.Tests.Repos;

/// <summary>
/// Tests for the <see cref="ClaimDocsEfRepo"/> class.
/// </summary>
public class ClaimDocsEfRepoTests
{
    [Fact]
    public async Task GetClaimDocuments_NoData_ReturnsEmpty()
    {
        // Arrange
        var db = GetInMemDb();
        var sut = new ClaimDocsEfRepo(db);
        var config = new RunConfig(DateTime.MinValue, DateTime.MaxValue, null);

        // Act
        var docs = await sut.GetClaimDocumentsAync(config);

        // Assert
        docs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClaimDocuments_WithMatches_ReturnsResults()
    {
        // Arrange
        var db = GetInMemDb(null, BuildDocumentRecord(123), BuildDocumentRecord(456));
        var sut = new ClaimDocsEfRepo(db);
        var config = new RunConfig(DateTime.MinValue, DateTime.MaxValue, new[] { 123, 456 });
        var expected = new[]
        {
            new ClaimDocument(123, default, default, default, default!),
            new ClaimDocument(456, default, default, default, default!),
        };

        // Act
        var docs = await sut.GetClaimDocumentsAync(config);

        // Assert
        docs.Should().BeEquivalentTo(expected, opts => opts.Including(d => d.PracticeId));
    }

    [Fact]
    public async Task GetClaimDocuments_EmptyPracticeIds_ReturnsEmpty()
    {
        // Arrange
        var db = GetInMemDb(null, BuildDocumentRecord());
        var sut = new ClaimDocsEfRepo(db);     
        var config = new RunConfig(DateTime.MinValue, DateTime.MaxValue, Array.Empty<int>());

        // Act
        var docs = await sut.GetClaimDocumentsAync(config);

        // Assert
        docs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClaimDocuments_WithDateFilter_ExcludesClaimsBeforeStart()
    {
        // Arrange
        var claimCreated = new DateTime(2020, 3, 19);
        var db = GetInMemDb(null, BuildDocumentRecord(claimCreatedOn: claimCreated));
        var sut = new ClaimDocsEfRepo(db);
        var config = new RunConfig(claimCreated.AddDays(1), DateTime.MaxValue, null);

        // Act
        var docs = await sut.GetClaimDocumentsAync(config);

        // Assert
        docs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClaimDocuments_WithDateFilter_ExcludesClaimsOnOrAfterEnd()
    {
        // Arrange
        var claimCreated = new DateTime(2020, 3, 19);
        var db = GetInMemDb(null, BuildDocumentRecord(claimCreatedOn: claimCreated));
        var sut = new ClaimDocsEfRepo(db);
        var config = new RunConfig(DateTime.MinValue, claimCreated, null);

        // Act
        var docs = await sut.GetClaimDocumentsAync(config);

        // Assert
        docs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClaimDocuments_NullPracticeIds_ReturnsAny()
    {
        // Arrange
        var practiceId = 123;
        var db = GetInMemDb(null, BuildDocumentRecord(practiceId));
        var sut = new ClaimDocsEfRepo(db);
        var config = new RunConfig(DateTime.MinValue, DateTime.MaxValue, null);
        var expected = new[] { new ClaimDocument(practiceId, default, default, default, default!) };

        // Act
        var docs = await sut.GetClaimDocumentsAync(config);

        // Assert
        docs.Should().BeEquivalentTo(expected, opts => opts.Including(d => d.PracticeId));
    }

    [Fact]
    public async Task GetClaimDocuments_WithPracticeIdsFilter_ReturnsMatchesOnly()
    {
        // Arrange
        var practiceId = 123;
        var db = GetInMemDb(null, BuildDocumentRecord(practiceId), BuildDocumentRecord(999));
        var sut = new ClaimDocsEfRepo(db);
        var config = new RunConfig(DateTime.MinValue, DateTime.MaxValue, new[] { practiceId });
        var expected = new[] { new ClaimDocument(practiceId, default, default, default, default!) };

        // Act
        var docs = await sut.GetClaimDocumentsAync(config);

        // Assert
        docs.Should().BeEquivalentTo(expected, opts => opts.Including(d => d.PracticeId));
    }

    [Fact]
    public async Task GetClaimDocuments_WithWidestCriteria_OmitsPreAuths()
    {
        // Arrange
        var db = GetInMemDb(null, BuildDocumentRecord(claimType: ClaimType.PreAuth));
        var sut = new ClaimDocsEfRepo(db);
        var config = new RunConfig(DateTime.MinValue, DateTime.MaxValue, null);

        // Act
        var docs = await sut.GetClaimDocumentsAync(config);

        // Assert
        docs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClaimDocuments_WithMatch_ReturnsFullExpectedData()
    {
        // Arrange
        var record = BuildDocumentRecord(123, DateTime.Today, ClaimType.Continuation, DocumentType.Invoice);
        var db = GetInMemDb(null, record);
        var sut = new ClaimDocsEfRepo(db);
        var config = new RunConfig(DateTime.MinValue, DateTime.MaxValue, null);
        var expected = new[]
        {
            new ClaimDocument(
                record.Claim!.PracticePolicy.PracticeId,
                record.Claim.Id,
                (ClaimType)record.Claim.TypeId,
                (DocumentType)record.TypeId,
                record.BlobUri)
        };

        // Act
        var docs = await sut.GetClaimDocumentsAync(config);

        // Assert
        docs.Should().BeEquivalentTo(expected);
    }

    private static SourceDbContext GetInMemDb(
        string? dbName = null,
        params ClaimUpload[] records)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SourceDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString());

        var db = new SourceDbContext(optionsBuilder.Options);
        db.AddRange(records);
        db.SaveChanges();
        return db;
    }

    private static ClaimUpload BuildDocumentRecord(
        int practiceId = 1,
        DateTime claimCreatedOn = default,
        ClaimType claimType = ClaimType.Claim,
        DocumentType documentType = DocumentType.MedicalHistory)
    {
        return new ClaimUpload
        {
            BlobUri = "https://tempuri.com/myblob.pdf",
            Filename = "myblob.pdf",
            Md5 = "md5",
            StorageId = "sid",
            UserId = "uid",
            TypeId = (int)documentType,
            Claim = new Claim
            {
                TypeId = (int)claimType,
                CreatedAt = claimCreatedOn == default ? new(2020, 3, 19) : claimCreatedOn,
                Diagnosis = "diagnosis",
                UserId = "uid",
                PracticePolicy = new PracticePolicy
                {
                    PracticeId = practiceId,
                }
            },
        };
    }
}
