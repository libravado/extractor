﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;

namespace ExtractorFunc.Services;

/// <inheritdoc cref="IBlobClientService"/>
public class BlobClientService : IBlobClientService
{
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
                await target.StartCopyFromUriAsync(source.Uri);
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
