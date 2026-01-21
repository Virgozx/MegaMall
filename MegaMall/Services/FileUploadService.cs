using MegaMall.Interfaces;

namespace MegaMall.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        // Các extension được phép cho hình ảnh
        private readonly string[] _imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        
        // Các extension được phép cho video
        private readonly string[] _videoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv" };

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Upload failed: File is null or empty");
                return null;
            }

            try
            {
                // Tạo thư mục nếu chưa tồn tại
                var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Tạo tên file unique
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về đường dẫn tương đối
                var relativePath = $"/uploads/{folder}/{uniqueFileName}";
                _logger.LogInformation($"File uploaded successfully: {relativePath}");
                
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file: {file.FileName}");
                return null;
            }
        }

        public async Task<List<string>> UploadFilesAsync(List<IFormFile> files, string folder)
        {
            var uploadedPaths = new List<string>();

            if (files == null || !files.Any())
            {
                return uploadedPaths;
            }

            foreach (var file in files)
            {
                var path = await UploadFileAsync(file, folder);
                if (!string.IsNullOrEmpty(path))
                {
                    uploadedPaths.Add(path);
                }
            }

            return uploadedPaths;
        }

        public Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Task.CompletedTask;
            }

            try
            {
                // Chuyển đường dẫn tương đối thành đường dẫn tuyệt đối
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/').Replace('/', '\\'));
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"File deleted successfully: {filePath}");
                }
                else
                {
                    _logger.LogWarning($"File not found for deletion: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
            }

            return Task.CompletedTask;
        }

        public bool IsImageFile(IFormFile file)
        {
            if (file == null)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _imageExtensions.Contains(extension);
        }

        public bool IsVideoFile(IFormFile file)
        {
            if (file == null)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _videoExtensions.Contains(extension);
        }
    }
}
