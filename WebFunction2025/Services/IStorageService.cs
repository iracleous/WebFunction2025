
namespace WebFunction2025.Services;

public interface IStorageService
{
    Task SaveStorage(string blobName, string fileContent);
}