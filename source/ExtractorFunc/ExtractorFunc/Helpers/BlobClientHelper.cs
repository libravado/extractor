using Azure.Identity;
using Azure.Storage.Blobs;

namespace ExtractorFunc.Helpers;

internal static class BlobClientHelper
{
    private const string DevConnection = "UseDevelopmentStorage=true";
    private const string AccountUrlFormat = "https://{0}.blob.core.windows.net";

    public static BlobServiceClient GetDevAccount() => new(DevConnection);

    public static BlobContainerClient GetDevContainer(string containerName)
        => new(DevConnection, containerName);

    public static BlobServiceClient GetHostedAccount(string accountName)
        => new(
            new Uri(GetAccountUrl(accountName)),
            new DefaultAzureCredential());

    public static BlobContainerClient GetHostedContainer(string accountName, string containerName)
        => new(
            new Uri(GetAccountUrl(accountName) + "/" + containerName),
            new DefaultAzureCredential());

    private static string GetAccountUrl(string accountName) 
        => string.Format(AccountUrlFormat, accountName);
}
