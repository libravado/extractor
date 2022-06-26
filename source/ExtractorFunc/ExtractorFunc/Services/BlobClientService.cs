using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace ExtractorFunc.Services;

/// <inheritdoc cref="IBlobClientService"/>
public class BlobClientService : IBlobClientService
{
    private const string DevConnection = "UseDevelopmentStorage=true";
    private const string AccountUrlFormat = "https://{0}.blob.core.windows.net";

    /// <inheritdoc/>
    public BlobServiceClient GetAccount(string? hostedAccountName)
        => hostedAccountName == null
            ? new(DevConnection)
            : new(
                new Uri(GetAccountUrl(hostedAccountName)),
                new DefaultAzureCredential());

    /// <inheritdoc/>
    public BlobContainerClient GetContainer(string containerName, string? hostedAccountName)
        => hostedAccountName == null
            ? new(DevConnection, containerName)
            : new(
                new Uri(GetAccountUrl(hostedAccountName + "/" + containerName)),
                new DefaultAzureCredential());

    /// <inheritdoc/>
    public async Task CopyBlobAsync(BlobClient source, BlobClient target)
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

    /// <inheritdoc/>
    public void CreateIfNotExists(BlobContainerClient container)
        => container.CreateIfNotExists();

    /// <inheritdoc/>
    public BlobClient GetBlobClient(BlobServiceClient source, string uriString)
    {
        var uri = new Uri(uriString);
        var containerName = uri.Segments.Skip(1).First().Trim('/');
        var blobFilePath = string.Join(string.Empty, uri.Segments.Skip(2));
        var container = source.GetBlobContainerClient(containerName);
        return container.GetBlobClient(blobFilePath);
    }

    /// <inheritdoc/>
    public async Task UploadJsonAsync(BlobContainerClient container, object data, string path)      
    {
        var jsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        jsonOpts.Converters.Add(new JsonStringEnumConverter());
        using var jsonStream = new MemoryStream();
        JsonSerializer.Serialize(jsonStream, data, jsonOpts);
        jsonStream.Seek(0, SeekOrigin.Begin);
        await container.UploadBlobAsync(path, jsonStream);
    }

    private static string GetAccountUrl(string accountName)
        => string.Format(AccountUrlFormat, accountName);
}
