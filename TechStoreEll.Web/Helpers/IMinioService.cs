namespace TechStoreEll.Web.Helpers;

public interface IMinioService
{
    Task<string> UploadImageAsync(IFormFile file, string bucketName = "storage");
    Task<bool> DeleteImageAsync(string objectName, string bucketName = "storage");
    Task EnsureBucketExistsAsync(string bucketName = "storage");
}