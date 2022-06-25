using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ExtractorFunc.Helpers;

internal static class EnvironmentExtensions
{
    private const string DevConnection = "UseDevelopmentStorage=true";
    private const string AccountUrlFormat = "https://{0}.blob.core.windows.net";

    public static BlobServiceClient GetSourceBlobAccount(
        this IHostingEnvironment env,
        IConfiguration config)
    {
        return env.IsDevelopment()
            ? new(DevConnection)
            : new(
                new Uri(GetAccountUrl(config["SourceDocsStorageAccountName"])),
                new DefaultAzureCredential());
    }

    public static BlobContainerClient GetExportContainer(
        this IHostingEnvironment env,
        IConfiguration config)
    {
        var containerName = config["ExportBlobContainerName"];
        return env.IsDevelopment()
            ? new(DevConnection, containerName)
            : new(
                new Uri(GetAccountUrl(config["ExportBlobStorageAccountName"]) + "/" + containerName),
                new DefaultAzureCredential());
    } 

    private static string GetAccountUrl(string accountName)
        => string.Format(AccountUrlFormat, accountName);
}
