using ExtractorFunc.Models;
using ExtractorFunc.Services;

namespace ExtractorFunc.Tests.Services;

/// <summary>
/// Tests for the <see cref="RunConfigParser"/> class.
/// </summary>
public class RunConfigParserTests
{
    [Fact]
    public void ParseCsv_ValidFormat_ParsedOk()
    {
        // Arrange
        var sut = new RunConfigParser();
        var expectedConfig = new RunConfig(new DateTime(2010, 1, 1), new DateTime(2022, 1, 1), new[] { 5883 });

        // Act
        var actualConfig = sut.ParseCsv(LoadConfig("sample.csv"));

        // Assert
        actualConfig.Should().BeEquivalentTo(expectedConfig);
    }

    [Fact]
    public void ParseJson_ValidFormat_ParsedOk()
    {
        // Arrange
        var sut = new RunConfigParser();
        var expectedConfig = new RunConfig(new DateTime(2010, 1, 1), new DateTime(2022, 1, 1), new[] { 5883 });

        // Act
        var actualConfig = sut.ParseJson(LoadConfig("sample.json"));

        // Assert
        actualConfig.Should().BeEquivalentTo(expectedConfig);
    }

    [Theory]
    [InlineData(".csv", false)]
    [InlineData(".CSV", false)]
    [InlineData(".json", true)]
    [InlineData(".JsOn", true)]
    public void Parse_ValidExtension_DoesNotError(string validExtension, bool useJson)
    {
        // Arrange
        IRunConfigParser sut = new RunConfigParser();
        var validFile = LoadConfig(useJson ? "sample.json" : "sample.csv");

        // Act
        var act = () => sut.Parse(validFile, validExtension);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".cvs")]
    [InlineData(".jason")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Parse_InvalidExtension_ThrowsExpectedError(string invalidExtension)
    {
        // Arrange
        IRunConfigParser sut = new RunConfigParser();
        var validFile = LoadConfig("sample.json");

        // Act
        var act = () => sut.Parse(validFile, invalidExtension);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Extension not supported: {invalidExtension}");
    }

    private static Stream LoadConfig(string testFileName)
    {
        var localSettingsPath = Path.Combine("TestConfigFiles", testFileName);
        var settingsPath = new FileInfo(localSettingsPath).FullName;
        return File.OpenRead(settingsPath);
    }
}
