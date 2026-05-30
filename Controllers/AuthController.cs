using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ClothingStoreWeb;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.ViewModels;
using ClothingStoreWeb.Services;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreWeb.Controllers
{
    public class AuthController : Controller
    {
        private const string PendingRegistrationKey = "PendingRegistration";
        private static readonly TimeSpan PendingRegistrationTimeout = TimeSpan.FromMinutes(30);

        private readonly ApplicationDbContext _context;
        private readonly IOtpService _otpService;
        private readonly IWebHostEnvironment _env;

        public AuthController(ApplicationDbContext context, IOtpService otpService, IWebHostEnvironment env)
        {
            _context = context;
            _otpService = otpService;
            _env = env;
        }

        private void HandleOtpSendResult(OtpSendResult result, string? messagePrefix = null)
        {
            var prefix = string.IsNullOrWhiteSpace(messagePrefix) ? string.Empty : messagePrefix + " ";

            if (result.EmailSent)
            {
                TempData["Info"] = $"{prefix}Mã OTP đã gửi tới email. Kiểm tra Hộp thư đến và cả Spam/Quảng cáo.";
            }
            else if (_env.IsDevelopment())
            {
                TempData["DevOtp"] = result.OtpCode;
                TempData["Info"] = $"{prefix}Chưa gửi được email — dùng mã OTP demo hiển thị bên dưới.";
            }
            else
            {
                TempData["Error"] = "Không gửi được email OTP. Vui lòng liên hệ quản trị viên.";
            }
        }

        private PendingRegistration? GetPendingRegistration()
        {
            return HttpContext.Session.GetObjectFromJson<PendingRegistration>(PendingRegistrationKey);
        }

        private void SavePendingRegistration(PendingRegistration pending)
        {
            HttpContext.Session.SetObjectAsJson(PendingRegistrationKey, pending);
        }

        private void ClearPendingRegistration()
        {
            HttpContext.Session.Remove(PendingRegistrationKey);
        }

        /// <summary>
        /// Xóa tài khoản Customer chưa xác thực email (dữ liệu cũ trước khi đổi luồng).
        /// </summary>
        private async Task CleanupUnverifiedUsersAsync(string username, string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var staleUsers = await _context.Users
                .Where(u => !u.EmailVerified
                            && u.Role == "Customer"
                            && (u.Username == username || u.Email.ToLower() == normalizedEmail))
                .ToListAsync();

            if (staleUsers.Count > 0)
            {
                _context.Users.RemoveRange(staleUsers);
                await _context.SaveChangesAsync();
            }
        }

        // --- ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = model.Email.Trim().ToLowerInvariant();

            var userExists = await _context.Users.AnyAsync(u =>
                u.Username == model.Username || u.Email.ToLower() == normalizedEmail);

            if (userExists)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc Email đã tồn tại.");
                return View(model);
            }

            await CleanupUnverifiedUsersAsync(model.Username, model.Email);

            var pending = new PendingRegistration
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                PasswordHash = PasswordHelper.Hash(model.Password),
                CreatedAt = DateTime.Now
            };
            SavePendingRegistration(pending);

            var otpResult = await _otpService.CreateAndSendOtpAsync(pending.Email, OtpPurpose.Register, "Xác thực đăng ký tài khoản");
            HandleOtpSendResult(otpResult, "Đăng ký thành công!");
            return RedirectToAction(nameof(VerifyEmail), new { email = pending.Email });
        }

        // --- XÁC THỰC EMAIL SAU ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult VerifyEmail(string? email)
        {
            var pending = GetPendingRegistration();
            var displayEmail = email ?? pending?.Email ?? string.Empty;
            return View(new VerifyEmailVM { Email = displayEmail });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var pending = GetPendingRegistration();
            if (pending == null)
            {
                ModelState.AddModelError("", "Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại.");
                return View(model);
            }

            if (DateTime.Now - pending.CreatedAt > PendingRegistrationTimeout)
            {
                ClearPendingRegistration();
                ModelState.AddModelError("", "Phiên đăng ký đã hết hạn (30 phút). Vui lòng đăng ký lại.");
                return View(model);
            }

            if (!string.Equals(pending.Email, model.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Email không khớp với phiên đăng ký. Vui lòng dùng đúng email đã đăng ký.");
                return View(model);
            }

            var isValid = await _otpService.ValidateOtpAsync(model.Email, model.OtpCode, OtpPurpose.Register);
            if (!isValid)
            {
                ModelState.AddModelError("", "Mã OTP không đúng hoặc đã hết hạn. Vui lòng thử lại.");
                return View(model);
            }

            var alreadyExists = await _context.Users.AnyAsync(u =>
                u.Username == pending.Username || u.Email.ToLower() == pending.Email.ToLower());

            if (alreadyExists)
            {
                ClearPendingRegistration();
                ModelState.AddModelError("", "Tài khoản đã tồn tại. Vui lòng đăng nhập.");
                return View(model);
            }

            var user = new User
            {
                Username = pending.Username,
                PasswordHash = pending.PasswordHash,
                Email = pending.Email,
                Role = "Customer",
                EmailVerified = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            ClearPendingRegistration();

            TempData["Success"] = "Xác thực email thành công! Tài khoản đã được tạo. Bạn có thể đăng nhập ngay.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> ResendRegisterOtp(string email)
        {
            var pending = GetPendingRegistration();
            if (pending == null || !string.Equals(pending.Email, email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Không tìm thấy phiên đăng ký. Vui lòng đăng ký lại.";
                return RedirectToAction(nameof(Register));
            }

            var otpResult = await _otpService.CreateAndSendOtpAsync(pending.Email, OtpPurpose.Register, "Xác thực đăng ký tài khoản");
            HandleOtpSendResult(otpResult);
            return RedirectToAction(nameof(VerifyEmail), new { email = pending.Email });
        }

        // --- ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var hashedPassword = PasswordHelper.Hash(model.Password);
            var user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Username == model.Username && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                var pending = GetPendingRegistration();
                if (pending != null && pending.Username == model.Username)
                {
                    ModelState.AddModelError("", "Bạn chưa xác thực OTP. Vui lòng hoàn tất xác thực email trước khi đăng nhập.");
                    TempData["Info"] = "Tài khoản chưa được tạo cho đến khi bạn nhập đúng mã OTP.";
                    return RedirectToAction(nameof(VerifyEmail), new { email = pending.Email });
                }

                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu.");
                return View(model);
            }

            if (user.IsLocked)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa do vi phạm chính sách hoặc spam hệ thống!");
                return View(model);
            }

            if (!user.EmailVerified)
            {
                ModelState.AddModelError("", "Tài khoản chưa xác thực email. Liên hệ quản trị viên.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.UserID.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        // --- QUÊN MẬT KHẨU ---
        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordVM());

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email.ToLower() == model.Email.Trim().ToLower() && u.EmailVerified);

            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy tài khoản đã xác thực với email này.");
                return View(model);
            }

            var otpResult = await _otpService.CreateAndSendOtpAsync(user.Email, OtpPurpose.ForgotPassword, "Đặt lại mật khẩu");
            HandleOtpSendResult(otpResult);
            return RedirectToAction(nameof(ResetPassword), new { email = user.Email });
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email)
        {
            return View(new ResetPasswordVM { Email = email ?? string.Empty });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email.ToLower() == model.Email.Trim().ToLower() && u.EmailVerified);

            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy tài khoản với email này.");
                return View(model);
            }

            var isValid = await _otpService.ValidateOtpAsync(model.Email, model.OtpCode, OtpPurpose.ForgotPassword);
            if (!isValid)
            {
                ModelState.AddModelError("", "Mã OTP không đúng hoặc đã hết hạn.");
                return View(model);
            }

            user.PasswordHash = PasswordHelper.Hash(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập với mật khẩu mới.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> ResendForgotPasswordOtp(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email.ToLower() == email.Trim().ToLower() && u.EmailVerified);

            if (user == null)
            {
                TempData["Error"] = "Không thể gửi lại mã OTP.";
                return RedirectToAction(nameof(ResetPassword), new { email });
            }

            var otpResult = await _otpService.CreateAndSendOtpAsync(user.Email, OtpPurpose.ForgotPassword, "Đặt lại mật khẩu");
            HandleOtpSendResult(otpResult);
            return RedirectToAction(nameof(ResetPassword), new { email = user.Email });
        }

        // --- ĐĂNG XUẤT ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
