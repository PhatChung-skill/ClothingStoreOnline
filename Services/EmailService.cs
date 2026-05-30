using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ClothingStoreWeb.Services
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Clothing Store";
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            // App Password Gmail có thể có dấu cách — bỏ hết khi gửi SMTP
            _settings.Password = _settings.Password?.Replace(" ", string.Empty) ?? string.Empty;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string purposeTitle)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) ||
                string.IsNullOrWhiteSpace(_settings.Username) ||
                string.IsNullOrWhiteSpace(_settings.Password) ||
                string.IsNullOrWhiteSpace(_settings.FromEmail))
            {
                _logger.LogWarning(
                    "SMTP chưa cấu hình. Mã OTP demo cho {Email}: {Otp}",
                    toEmail, otpCode);
                return false;
            }

            var subject = $"{purposeTitle} - Mã OTP Clothing Store";
            var htmlBody = $"""
                <div style="font-family:Arial,sans-serif;max-width:480px;margin:0 auto;padding:24px;border:1px solid #f1d6d8;border-radius:12px;">
                    <h2 style="color:#d71920;margin-top:0;">{purposeTitle}</h2>
                    <p>Xin chào,</p>
                    <p>Mã xác thực OTP của bạn là:</p>
                    <p style="font-size:32px;font-weight:bold;letter-spacing:8px;color:#d71920;text-align:center;">{otpCode}</p>
                    <p style="color:#666;font-size:14px;">Mã có hiệu lực trong <strong>10 phút</strong>. Không chia sẻ mã này với bất kỳ ai.</p>
                    <p style="color:#999;font-size:12px;">Nếu không thấy email, vui lòng kiểm tra thư mục Spam/Quảng cáo.</p>
                </div>
                """;
            var textBody = $"{purposeTitle}\n\nMã OTP: {otpCode}\n\nMã có hiệu lực 10 phút.\nClothing Store";

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));

                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Đã gửi OTP tới {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gửi email OTP thất bại tới {Email}. Mã demo: {Otp}", toEmail, otpCode);
                return false;
            }
        }
    }
}
