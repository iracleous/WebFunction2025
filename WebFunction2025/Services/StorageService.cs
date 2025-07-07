using Azure.Storage.Blobs;
using System.Text;

namespace WebFunction2025.Services;

public class StorageService : IStorageService
{
    private readonly string? _connectionString = Environment.GetEnvironmentVariable("MyStorageConnectionString");

    private readonly string _containerName = "myblobcontainer"; // e.g., "mydata"

    public async Task SaveStorage(string blobName, string fileContent)
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Storage connection string is not configured.");
        }

        try
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

            // Get a reference to the container
            // If the container does not exist, you can create it with .CreateIfNotExistsAsync()
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(); // Ensure container exists

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Convert the string content to a byte array for uploading
            byte[] byteArray = Encoding.UTF8.GetBytes(fileContent);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                // Upload the stream to the blob
                await blobClient.UploadAsync(stream, overwrite: true); // overwrite: true allows overwriting existing blobs
            }
        }
        catch (Azure.RequestFailedException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
