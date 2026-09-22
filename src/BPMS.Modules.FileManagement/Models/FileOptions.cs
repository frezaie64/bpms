namespace BPMS.Modules.FileManagement.Models;

public class MinioOptions
{
    public const string SectionName = "Minio";
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public string BucketName { get; set; } = "bpms-files";
}

public class FileValidationOptions
{
    public const string SectionName = "FileValidation";
    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
        ".pdf",
        ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv",
        ".zip", ".rar", ".7z", ".tar", ".gz"
    ];
}