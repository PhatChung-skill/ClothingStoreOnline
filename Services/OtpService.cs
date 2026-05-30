using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreWeb.Services
{
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);

        public OtpService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<OtpSendResult> CreateAndSendOtpAsync(string email, string purpose, string purposeTitle)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var oldOtps = await _context.EmailOtps
                .Where(o => o.Email == normalizedEmail && o.Purpose == purpose && !o.IsUsed)
                .ToListAsync();

            foreach (var old in oldOtps)
            {
                old.IsUsed = true;
            }

            var otpCode = PasswordHelper.GenerateOtp();
            var otp = new EmailOtp
            {
                Email = normalizedEmail,
                OtpHash = PasswordHelper.Hash(otpCode),
                Purpose = purpose,
                ExpiresAt = DateTime.Now.Add(OtpLifetime),
                IsUsed = false
            };

            _context.EmailOtps.Add(otp);
            await _context.SaveChangesAsync();

            var emailSent = await _emailService.SendOtpEmailAsync(normalizedEmail, otpCode, purposeTitle);

            return new OtpSendResult
            {
                OtpCode = otpCode,
                EmailSent = emailSent,
                ErrorMessage = emailSent ? null : "Chưa gửi được email. Kiểm tra cấu hình SMTP hoặc dùng mã demo bên dưới."
            };
        }

        public async Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var otpHash = PasswordHelper.Hash(otpCode.Trim());

            var record = await _context.EmailOtps
                .Where(o => o.Email == normalizedEmail
                            && o.Purpose == purpose
                            && !o.IsUsed
                            && o.ExpiresAt >= DateTime.Now)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (record == null || record.OtpHash != otpHash)
            {
                return false;
            }

            record.IsUsed = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
