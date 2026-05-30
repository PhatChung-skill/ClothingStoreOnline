namespace ClothingStoreWeb.ViewModels
{
    /// <summary>
    /// Lưu tạm trong Session cho đến khi xác thực OTP thành công mới tạo User trong DB.
    /// </summary>
    public class PendingRegistration
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
