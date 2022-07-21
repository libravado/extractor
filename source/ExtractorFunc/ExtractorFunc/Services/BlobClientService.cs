using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;

namespace ExtractorFunc.Services;

/// <inheritdoc cref="IBlobClientService"/>
public class BlobClientService : IBlobClientService
{
    private readonly ILogger<BlobClientService> logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="BlobClientService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public BlobClientService(ILogger<BlobClientService> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task CopyBlobAsync(BlobClient source, BlobClient target)
    {
        if (!await source.ExistsAsync())
        {
            throw new ArgumentException("Source blob not found.");
        }

        if (!await target.ExistsAsync())
        {
            try
            {
                //var sasUri = source.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(60));
                var copyOperation = await target.StartCopyFromUriAsync(source.Uri);

                // Display the status of the blob as it is copied
                while (!copyOperation.HasCompleted)
                {
                    var copied = await copyOperation.WaitForCompletionAsync();
                    logger.LogDebug($"Blob: {target.Name}, Copied: {copied} of ???");
                    await Task.Delay(1000);
                }

                Console.WriteLine($"Blob: {target.Name} Complete");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Blob copy failed, dude", ex);
            }
        }
    }

    /// <inheritdoc/>
    public void CreateIfNotExists(BlobContainerClient container)
    {
        container.CreateIfNotExists();
    }

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
}
