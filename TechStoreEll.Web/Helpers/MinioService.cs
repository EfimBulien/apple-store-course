using Minio;
using Minio.DataModel.Args;

namespace TechStoreEll.Web.Helpers;

public class MinioService : IMinioService
{
    private readonly MinioClient? _minioClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MinioService> _logger;

    public MinioService(IConfiguration configuration, ILogger<MinioService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var endpoint = _configuration["MinIO:Endpoint"] ?? "localhost:9000";
        var accessKey = _configuration["MinIO:AccessKey"] ?? "minioadmin";
        var secretKey = _configuration["MinIO:SecretKey"] ?? "minioadmin";

        _minioClient = (MinioClient?)new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false)
            .Build();
    }

    public async Task EnsureBucketExistsAsync(string bucketName = "storage")
    {
        try
        {
            var bucketExists = await _minioClient?
                .BucketExistsAsync(new BucketExistsArgs()
                .WithBucket(bucketName))!;
            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
                _logger.LogInformation("{BucketName} created successfully", bucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring bucket exists: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<string> UploadImageAsync(IFormFile file, string bucketName = "storage")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");
        
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Invalid image format");

        await EnsureBucketExistsAsync(bucketName);

        var objectName = $"{Guid.NewGuid()}{extension}";

        await using var stream = file.OpenReadStream();
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(file.ContentType);

        await _minioClient?.PutObjectAsync(putObjectArgs)!;
        
        var baseUrl = _configuration["MinIO:PublicUrl"] ?? "http://localhost:9000";
        return $"{baseUrl}/{bucketName}/{objectName}";
    }

    public async Task<bool> DeleteImageAsync(string objectName, string bucketName = "storage")
    {
        try
        {
            await _minioClient?.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName))!;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {ObjectName}", objectName);
            return false;
        }
    }
}