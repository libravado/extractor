using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace ExtractorFunc.Helpers;

internal static class BlobClientHelper
{
    private const string DevConnection = "UseDevelopmentStorage=true";
    private const string AccountUrlFormat = "https://{0}.blob.core.windows.net";

    public static async Task CopyToAsync(this BlobClient source, BlobClient target)
    {
        if (!await source.ExistsAsync())
        {
            throw new ArgumentException("Source blob not found.");
        }

        if (!await target.ExistsAsync())
        {
            var lease = source.GetBlobLeaseClient();
            try
            {
                await lease.AcquireAsync(TimeSpan.FromSeconds(-1));
                await target.StartCopyFromUriAsync(source.Uri);
            }
            finally
            {
                BlobProperties sourceProperties = await source.GetPropertiesAsync();
                if (sourceProperties.LeaseState == LeaseState.Leased)
                {
                    await lease.BreakAsync();
                }
            }
        }
    }

    public static async Task SendJsonAsync(this BlobContainerClient container, object data, string path)
    {
        using var jsonStream = new MemoryStream();
        var jsonOpts = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        jsonOpts.Converters.Add(new JsonStringEnumConverter());
        JsonSerializer.Serialize(jsonStream, data, jsonOpts);
        jsonStream.Seek(0, SeekOrigin.Begin);
        await container.UploadBlobAsync(path, jsonStream);
    }

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
