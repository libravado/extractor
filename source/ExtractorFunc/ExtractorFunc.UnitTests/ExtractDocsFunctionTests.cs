using ExtractorFunc.Models;
using ExtractorFunc.Repos;
using ExtractorFunc.Services;

namespace ExtractorFunc.Tests
{
    /// <summary>
    /// Tests for the <see cref="ExtractDocsFunction"/> class.
    /// </summary>
    public class ExtractDocsFunctionTests
    {
        [Theory]
        [InlineData("file.txt", ".txt")]
        [InlineData("file.CvS", ".CvS")]
        [InlineData("file", "")]
        [InlineData("file.csv", ".csv")]
        [InlineData("file.JSON", ".JSON")]
        [InlineData("file.json", ".json")]
        public async Task Run_VaryingFileName_ExpectedExtension(string fileName, string ext)
        {
            // Arrange
            var sut = GetSut(out var mocks);

            // Act
            await sut.Run(null!, fileName);

            // Assert
            mocks.MockRunConfigParser.Verify(m => m.Parse(null!, ext), Times.Once());
        }

        [Fact]
        public async Task Run_ParsedConfig_PassedToQuery()
        {
            // Arrange
            var mockConfig = new RunConfig(DateTime.Today.AddDays(-9), DateTime.Today.AddDays(-5), new[] { 22 });
            var sut = GetSut(out var mocks);
            mocks.MockRunConfigParser
                .Setup(m => m.Parse(It.IsAny<Stream>(), It.IsAny<string>()))
                .Returns(mockConfig);

            // Act
            await sut.Run(null!, "file");

            // Assert
            mocks.MockClaimDocsRepo.Verify(m => m.GetClaimDocumentsAync(mockConfig), Times.Once());
        }

        [Fact]
        public async Task Run_MultipleResults_EachHasCopyAttempted()
        {
            // Arrange
            var mockDoc = new ClaimDocument(default, default, default, default, default!);
            var mockCount = 33;
            var sut = GetSut(out var mocks);
            mocks.MockClaimDocsRepo
                .Setup(m => m.GetClaimDocumentsAync(It.IsAny<RunConfig>()))
                .ReturnsAsync(Enumerable.Range(0, mockCount).Select(i => mockDoc).ToList());

            // Act
            await sut.Run(null!, "file");

            // Assert
            mocks.MockDataExtractRepo.Verify(m => m.CopyDocumentAsync(It.IsAny<ClaimDocument>()), Times.Exactly(mockCount));
        }

        [Fact]
        public async Task Run_ResultsTracked_AsExpected()
        {
            // Arrange
            var sut = GetSut(out var mocks);
            mocks.MockClaimDocsRepo
                .Setup(m => m.GetClaimDocumentsAync(It.IsAny<RunConfig>()))
                .ReturnsAsync(new List<ClaimDocument>
                {
                    new ClaimDocument(1, 1, ClaimType.Claim, DocumentType.Invoice, "blob1"),
                    new ClaimDocument(2, 2, ClaimType.PreAuth, DocumentType.MedicalHistory, "blob2"),
                    new ClaimDocument(3, 3, ClaimType.Claim, DocumentType.MedicalHistory, "blob3"),
                });

            RunResult actualRunResult = default!;
            mocks.MockDataExtractRepo
                .Setup(m => m.UploadResultsAsync(It.IsAny<RunResult>()))
                .Callback<RunResult>(r => actualRunResult = r);

            var expectedResult = new RunResult
            {
                RanToCompletion = true,
                FailedUris = new List<string>(),
                DocumentsFound = 3,
                DocumentsFoundByClaimType = new()
                {
                    { ClaimType.PreAuth, 1 },
                    { ClaimType.Claim, 2 },
                },
                DocumentsExportedByClaimType = new()
                {
                    { ClaimType.PreAuth, 1 },
                    { ClaimType.Claim, 2 },
                },
            };

            // Act
            await sut.Run(null!, "file");

            // Assert
            mocks.MockDataExtractRepo.Verify(m => m.UploadResultsAsync(It.IsAny<RunResult>()), Times.Once());
            actualRunResult.Should().BeEquivalentTo(expectedResult, opts => opts.Excluding(r => r.LocalTimestamp));
        }

        [Fact]
        public async Task Run_ErrorsInCopying_MarkedAsCompletedWithFailedUris()
        {
            // Arrange
            var mockQueryResults = new List<ClaimDocument>
            {
                new ClaimDocument(default, default, default, default, default!),
                new ClaimDocument(default, default, default, default, default!),
            };
            var sut = GetSut(out var mocks);
            mocks.MockClaimDocsRepo
                .Setup(m => m.GetClaimDocumentsAync(It.IsAny<RunConfig>()))
                .ReturnsAsync(mockQueryResults);
            mocks.MockDataExtractRepo
                .Setup(m => m.CopyDocumentAsync(It.IsAny<ClaimDocument>()))
                .ThrowsAsync(new Exception("fail"));

            RunResult actualRunResult = default!;
            mocks.MockDataExtractRepo
                .Setup(m => m.UploadResultsAsync(It.IsAny<RunResult>()))
                .Callback<RunResult>(r => actualRunResult = r);

            // Act
            await sut.Run(null!, "file");

            // Assert
            mocks.MockDataExtractRepo.Verify(m => m.UploadResultsAsync(It.IsAny<RunResult>()), Times.Once());
            actualRunResult.RanToCompletion.Should().BeTrue();
            actualRunResult.ErrorMessage.Should().BeNull();
            actualRunResult.FailedUriCount.Should().Be(mockQueryResults.Count);
        }

        [Fact]
        public async Task Run_UnexpectedQueryError_UploadsResultsButMarkedAsNotCompleted()
        {
            // Arrange
            var sut = GetSut(out var mocks);
            mocks.MockClaimDocsRepo
                .Setup(m => m.GetClaimDocumentsAync(It.IsAny<RunConfig>()))
                .ThrowsAsync(new Exception("fail"));

            RunResult actualRunResult = default!;
            mocks.MockDataExtractRepo
                .Setup(m => m.UploadResultsAsync(It.IsAny<RunResult>()))
                .Callback<RunResult>(r => actualRunResult = r);

            // Act
            await sut.Run(null!, "file");

            // Assert
            mocks.MockDataExtractRepo.Verify(m => m.UploadResultsAsync(It.IsAny<RunResult>()), Times.Once());
            actualRunResult.RanToCompletion.Should().BeFalse();
            actualRunResult.ErrorMessage.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Run_UnexpectedParsingError_UploadsResultsButMarkedAsNotCompleted()
        {
            // Arrange
            var sut = GetSut(out var mocks);
            mocks.MockRunConfigParser
                .Setup(m => m.Parse(It.IsAny<Stream>(), It.IsAny<string>()))
                .Throws(new Exception("fail"));

            RunResult actualRunResult = default!;
            mocks.MockDataExtractRepo
                .Setup(m => m.UploadResultsAsync(It.IsAny<RunResult>()))
                .Callback<RunResult>(r => actualRunResult = r);

            // Act
            await sut.Run(null!, "file");

            // Assert
            mocks.MockDataExtractRepo.Verify(m => m.UploadResultsAsync(It.IsAny<RunResult>()), Times.Once());
            actualRunResult.RanToCompletion.Should().BeFalse();
            actualRunResult.ErrorMessage.Should().NotBeEmpty();
        }

        private static ExtractDocsFunction GetSut(out BagOfMocks mocks)
        {
            mocks = new BagOfMocks(
                new Mock<IDataExtractRepo>(),
                new Mock<IClaimDocsRepo>(),
                new Mock<IRunConfigParser>());

            return new ExtractDocsFunction(
                mocks.MockDataExtractRepo.Object,
                mocks.MockClaimDocsRepo.Object,
                mocks.MockRunConfigParser.Object);
        }

        private record BagOfMocks(
            Mock<IDataExtractRepo> MockDataExtractRepo,
            Mock<IClaimDocsRepo> MockClaimDocsRepo,
            Mock<IRunConfigParser> MockRunConfigParser);
    }
}