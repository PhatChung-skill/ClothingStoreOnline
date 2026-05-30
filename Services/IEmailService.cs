namespace ClothingStoreWeb.Services
{
    public interface IEmailService
    {
        /// <returns>true nếu gửi SMTP thành công</returns>
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string purposeTitle);
    }
}
