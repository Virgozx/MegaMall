namespace MegaMall.Interfaces
{
    public interface IFileUploadService
    {
        /// <summary>
        /// Upload một file lên server
        /// </summary>
        /// <param name="file">File cần upload</param>
        /// <param name="folder">Thư mục đích (ví dụ: "products", "avatars")</param>
        /// <returns>Đường dẫn tương đối của file đã upload (ví dụ: "/uploads/products/xyz.jpg")</returns>
        Task<string> UploadFileAsync(IFormFile file, string folder);

        /// <summary>
        /// Upload nhiều file cùng lúc
        /// </summary>
        /// <param name="files">Danh sách file cần upload</param>
        /// <param name="folder">Thư mục đích</param>
        /// <returns>Danh sách đường dẫn tương đối của các file đã upload</returns>
        Task<List<string>> UploadFilesAsync(List<IFormFile> files, string folder);

        /// <summary>
        /// Xóa file khỏi server
        /// </summary>
        /// <param name="filePath">Đường dẫn tương đối của file (ví dụ: "/uploads/products/xyz.jpg")</param>
        Task DeleteFileAsync(string filePath);

        /// <summary>
        /// Kiểm tra xem file có phải là hình ảnh không
        /// </summary>
        bool IsImageFile(IFormFile file);

        /// <summary>
        /// Kiểm tra xem file có phải là video không
        /// </summary>
        bool IsVideoFile(IFormFile file);
    }
}
