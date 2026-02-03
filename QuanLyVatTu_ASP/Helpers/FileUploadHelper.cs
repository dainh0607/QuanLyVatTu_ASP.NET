namespace QuanLyVatTu_ASP.Helpers
{
    public static class FileUploadHelper
    {
        /// <summary>
        /// Upload file và trả về đường dẫn tương đối
        /// </summary>
        /// <param name="file">File upload</param>
        /// <param name="webRootPath">wwwroot path</param>
        /// <param name="subFolder">Thư mục con (vd: "images/khachhang")</param>
        /// <returns>Đường dẫn tương đối từ wwwroot</returns>
        public static async Task<string?> UploadFileAsync(IFormFile? file, string webRootPath, string subFolder)
        {
            if (file == null || file.Length == 0) return null;

            // Tạo đường dẫn thư mục
            var uploadPath = Path.Combine(webRootPath, subFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Tạo tên file duy nhất
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối (cho URL)
            return $"/{subFolder.Replace("\\", "/")}/{fileName}";
        }

        /// <summary>
        /// Xóa file cũ (nếu có)
        /// </summary>
        public static void DeleteFile(string? relativePath, string webRootPath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            var filePath = Path.Combine(webRootPath, relativePath.TrimStart('/').Replace("/", "\\"));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
