using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace BPMS.Modules.FileManagement.Services;

public interface IFileStorageService
{
    Task UploadAsync(string objectName, string bucketName, Stream data, string contentType, CancellationToken ct = default);
    Task<Stream?> DownloadAsync(string objectName, string bucketName, CancellationToken ct = default);
    Task DeleteAsync(string objectName, string bucketName, CancellationToken ct = default);
    Task EnsureBucketExistsAsync(string bucketName, CancellationToken ct = default);
}

public class FileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;

    public FileStorageService(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task EnsureBucketExistsAsync(string bucketName, CancellationToken ct = default)
    {
        var args = new BucketExistsArgs().WithBucket(bucketName);
        var exists = await _minioClient.BucketExistsAsync(args, ct);
        if (!exists)
        {
            var makeArgs = new MakeBucketArgs().WithBucket(bucketName);
            await _minioClient.MakeBucketAsync(makeArgs, ct);
        }
    }

    public async Task UploadAsync(string objectName, string bucketName, Stream data, string contentType, CancellationToken ct = default)
    {
        var args = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType);
        await _minioClient.PutObjectAsync(args, ct);
    }

    public async Task<Stream?> DownloadAsync(string objectName, string bucketName, CancellationToken ct = default)
    {
        try
        {
            var memoryStream = new MemoryStream();
            var args = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));
            await _minioClient.GetObjectAsync(args, ct);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string objectName, string bucketName, CancellationToken ct = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);
        await _minioClient.RemoveObjectAsync(args, ct);
    }
}