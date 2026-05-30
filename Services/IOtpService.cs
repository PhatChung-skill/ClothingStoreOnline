namespace ClothingStoreWeb.Services
{
    public interface IOtpService
    {
        Task<OtpSendResult> CreateAndSendOtpAsync(string email, string purpose, string purposeTitle);
        Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose);
    }
}
