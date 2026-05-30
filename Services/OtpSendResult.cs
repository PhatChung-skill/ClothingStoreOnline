namespace ClothingStoreWeb.Services
{
    public class OtpSendResult
    {
        public string OtpCode { get; set; } = string.Empty;
        public bool EmailSent { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
