namespace QuanLyVatTu_ASP.Models
{
    /// <summary>
    /// Kết quả trả về từ Service layer (không có data)
    /// </summary>
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ServiceResult Ok(string message = "") => new() { Success = true, Message = message };
        public static ServiceResult Fail(string message) => new() { Success = false, Message = message };
    }

    /// <summary>
    /// Kết quả trả về từ Service layer (có data generic)
    /// </summary>
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "") => new() { Success = true, Data = data, Message = message };
        public static ServiceResult<T> Fail(string message) => new() { Success = false, Message = message };
    }
}
