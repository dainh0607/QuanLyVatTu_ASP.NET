using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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
            var uploadPath = Path.Combine(webRootPath, subFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };

            if (imageExtensions.Contains(extension))
            {
                try
                {
                    // Tên file mới với đuôi .webp
                    var fileName = $"{Guid.NewGuid()}.webp";
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Load ảnh từ stream
                    using (var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream()))
                    {
                        // Logic Resize: Nếu chiều rộng > 800px thì resize về 800px (giữ tỷ lệ)
                        const int maxDimension = 800;
                        if (image.Width > maxDimension || image.Height > maxDimension)
                        {
                           // Tính toán kích thước mới giữ nguyên tỷ lệ
                           int newWidth, newHeight;
                           if (image.Width > image.Height)
                           {
                               newWidth = maxDimension;
                               newHeight = (int)((float)image.Height / image.Width * maxDimension);
                           }
                           else
                           {
                               newHeight = maxDimension;
                               newWidth = (int)((float)image.Width / image.Height * maxDimension);
                           }

                           image.Mutate(x => x.Resize(newWidth, newHeight));
                        }

                        // Lưu ảnh dưới dạng WebP với chất lượng 75
                        var encoder = new SixLabors.ImageSharp.Formats.Webp.WebpEncoder()
                        {
                            Quality = 75
                        };
                        await image.SaveAsync(filePath, encoder);
                    }

                    // Trả về đường dẫn WebP
                    return $"/{subFolder.Replace("\\", "/")}/{fileName}";
                }
                catch (Exception)
                {
                    // Nếu lỗi xử lý ảnh (vd file lỗi), fallback về save thường
                    // (Hoặc có thể log lỗi và return null hoặc throw tùy yêu cầu)
                }
            }

            // --- Logic Fallback / File thường (giữ nguyên logic cũ) ---
            var defaultFileName = $"{Guid.NewGuid()}{extension}";
            var defaultFilePath = Path.Combine(uploadPath, defaultFileName);

            using (var stream = new FileStream(defaultFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{subFolder.Replace("\\", "/")}/{defaultFileName}";
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
