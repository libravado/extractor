using Azure.Storage.Blobs;

namespace ExtractorFunc.Services;

/// <summary>
/// Communication with blobs.
/// </summary>
public interface IBlobClientService
{
    /// <summary>
    /// Gets a storage account client.
    /// </summary>
    /// <param name="hostedAccountName">The account name (if hosted). If null, a
    /// client for local emulation is returned.</param>
    /// <returns>A storage account client.</returns>
    public BlobServiceClient GetAccount(string? hostedAccountName);

    /// <summary>
    /// Gets a storage account container client.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <param name="hostedAccountName">The account name (if hosted). If null, a
    /// client for local emulation is returned.</param>
    /// <returns>A storage account container client.</returns>
    public BlobContainerClient GetContainer(string containerName, string? hostedAccountName);

    /// <summary>
    /// Creates a container if it does not already exist.
    /// </summary>
    /// <param name="container">The container to ensure the existence of.</param>
    public void CreateIfNotExists(BlobContainerClient container);

    /// <summary>
    /// Copies a blob to another location. The blobs do not necessary have to be
    /// in the same container (or indeed the same account).
    /// </summary>
    /// <param name="source">The source blob.</param>
    /// <param name="target">The target blob.</param>
    /// <exception cref="ArgumentException">Source blob not found.</exception>
    public Task CopyBlobAsync(BlobClient source, BlobClient target);

    /// <summary>
    /// Gets a blob client for the specified container based on the blob uri.
    /// The account specified in the original uri is entirely disregarded.
    /// </summary>
    /// <param name="source">The account service client.</param>
    /// <param name="uriString">The blob uri as a string.</param>
    /// <returns>A blob client.</returns>
    public BlobClient GetBlobClient(BlobServiceClient source, string uriString);

    /// <summary>
    /// Serialises an object as json and uploads it to the specified container.
    /// </summary>
    /// <param name="container">The target container.</param>
    /// <param name="data">The payload data.</param>
    /// <param name="path">The target blob path.</param>
    public Task UploadJsonAsync(BlobContainerClient container, object data, string path);
}
