using ExtractorFunc.Services;

namespace ExtractorFunc.Tests.Services;

/// <summary>
/// Tests for the <see cref="BlobClientService"/> class.
/// </summary>
public class BlobClientServiceTests
{
    [Fact]
    public void GetAccount_NoAccountName_ReturnsDevStoreAccount()
    {
        // Arrange
        var sut = new BlobClientService();

        // Act
        var account = sut.GetAccount(null);

        // Assert
        account.AccountName.Should().StartWith("devstore");
    }

    [Fact]
    public void GetAccount_WithAccountName_ReturnsHostedStoreAccount()
    {
        // Arrange
        var sut = new BlobClientService();

        // Act
        var account = sut.GetAccount("madeupaccount");

        // Assert
        account.Uri.Host.Should().EndWith(".blob.core.windows.net");
    }

    [Fact]
    public void GetContainer_NoAccountName_ReturnsDevStoreContainer()
    {
        // Arrange
        var sut = new BlobClientService();
        var containerName = "containerymccontainerface";

        // Act
        var container = sut.GetContainer(containerName, null);

        // Assert
        container.AccountName.Should().StartWith("devstore");
        container.Name.Should().Be(containerName);
    }

    [Fact]
    public void GetContainer_WithAccountName_ReturnsHostedContainer()
    {
        // Arrange
        var sut = new BlobClientService();
        var containerName = "containerymccontainerface";
        var hostedSuffix = ".blob.core.windows.net";

        // Act
        var container = sut.GetContainer(containerName, "madeupaccount");

        // Assert
        container.Uri.ToString().Should().StartWith($"https://madeupaccount{hostedSuffix}");
        container.Name.Should().Be(containerName);
    }
}
