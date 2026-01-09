// Models/FileUpload/FileUploadModels.cs
namespace DevoteeAnusanga.Models.FileUpload
{
    // POST /api/files/avatar - Upload avatar response
    public class AvatarUploadResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AvatarUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
    }

    // POST /api/files/upload - Generic file upload response
    public class FileUploadResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
    }

    // File validation settings
    public class AvatarUploadSettings
    {
        public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024; // 2MB
        public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp" };
        public string[] AllowedMimeTypes { get; set; } = { "image/jpeg", "image/png", "image/webp" };
    }
}
