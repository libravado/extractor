using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using ExtractorFunc.Models;
using ExtractorFunc.Repos;
using ExtractorFunc.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace ExtractorFunc.Tests.Repos;

/// <summary>
/// Tests for the <see cref="DataExtractRepo"/> class.
/// </summary>
public class DataExtractRepoTests
{
    [Theory]
    [InlineData(2, 3, ClaimType.Claim, DocumentType.Estimate, "http://blob.io/1.pdf", "practice-2/Claim/claim-3/Estimate/1.pdf")]
    [InlineData(9, 1, ClaimType.PreAuth, DocumentType.Note, "http://blob.io/dir1/dir2/dir3/33.txt", "practice-9/PreAuth/claim-1/Note/33.txt")]
    [InlineData(4, 99, ClaimType.Claim, DocumentType.Invoice, "c:\\dir3\\mytxt.docx", "practice-4/Claim/claim-99/Invoice/mytxt.docx")]
    [InlineData(3, 22, ClaimType.Continuation, DocumentType.Note, "http://blob.io/3.xyz", "practice-3/Continuation/claim-22/Note/3.xyz")]
    public void BuildDocumentPath_WithDocument_FormatAsExpected(
        int practiceId,
        int claimId,
        ClaimType claimType,
        DocumentType documentType,
        string blobUri,
        string expectedPath)
    {
        // Arrange
        var sut = GetSut(out _);
        var doc = new ClaimDocument(practiceId, claimId, claimType, documentType, blobUri);

        // Act
        var actualPath = sut.BuildDocumentPath(doc);

        // Assert
        actualPath.Should().Be(expectedPath);
    }

    [Fact]
    public async Task CopyDocumentAsync_WithoutError_CallsCopyBlobAsync()
    {
        // Arrange
        var sut = GetSut(out var blobService);

        // Act
        await sut.CopyDocumentAsync(new ClaimDocument(default, default, default, default, "ftp://f/1.txt"));

        // Assert
        blobService.Verify(
            m => m.CopyBlobAsync(It.IsAny<BlobClient>(), It.IsAny<BlobClient>()),
            Times.Once());
    }

    [Fact]
    public async Task CopyDocumentAsync_ErrorGettingClient_DoesNotCallCopyBlobAsync()
    {
        // Arrange
        var sut = GetSut(out var blobService);
        blobService
            .Setup(m => m.GetBlobClient(It.IsAny<BlobServiceClient>(), It.IsAny<string>()))
            .Throws(new Exception("fail"));

        // Act
        var act = () => sut.CopyDocumentAsync(new ClaimDocument(default, default, default, default, "ftp://f/1.txt"));

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("fail");
        blobService.Verify(
            m => m.CopyBlobAsync(It.IsAny<BlobClient>(), It.IsAny<BlobClient>()),
            Times.Never());
    }

    [Fact]
    public async Task UploadResultsAsync_WithResult_HasExpectedPath()
    {
        // Arrange
        var sut = GetSut(out var blobService);
        var mockResult = new RunResult();
        var expectedName = $"runs/{mockResult.LocalTimestamp:o}.results.json";

        // Act
        await sut.UploadResultsAsync(mockResult);

        // Assert
        blobService.Verify(
            m => m.UploadJsonAsync(It.IsAny<BlobContainerClient>(), It.IsAny<object>(), expectedName),
            Times.Once());
    }

    private static DataExtractRepo GetSut(out Mock<IBlobClientService> mockService)
    {
        var exportContainerName = "export";

        mockService = new Mock<IBlobClientService>();
        mockService
            .Setup(m => m.GetContainer(exportContainerName, It.IsAny<string>()))
            .Returns(new BlobContainerClient("UseDevelopmentStorage=true", "export"));

        var mockEnv = new Mock<IHostingEnvironment>();
        mockEnv.Setup(m => m.EnvironmentName).Returns("Development");

        var mockLogger = new Mock<ILogger<DataExtractRepo>>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ExportBlobContainerName", exportContainerName },
            }).Build();

        return new DataExtractRepo(mockEnv.Object, config, mockService.Object, mockLogger.Object);
    }
}
