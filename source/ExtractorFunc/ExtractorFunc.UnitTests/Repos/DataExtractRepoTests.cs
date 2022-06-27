using ExtractorFunc.Repos;
using ExtractorFunc.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ExtractorFunc.UnitTests.Repos;

/// <summary>
/// Tests for the <see cref="DataExtractRepo"/> class.
/// </summary>
public class DataExtractRepoTests
{
    [Fact]
    public void Test1()
    { }

    private static DataExtractRepo GetSut(out Mock<IBlobClientService> mockService)
    {
        mockService = new Mock<IBlobClientService>();
        var mockEnv = new Mock<IHostingEnvironment>();
        mockEnv.Setup(m => m.IsDevelopment()).Returns(true);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ExportBlobContainerName", "export" },
            }).Build();

        return new DataExtractRepo(mockEnv.Object, config, mockService.Object);
    }
}
