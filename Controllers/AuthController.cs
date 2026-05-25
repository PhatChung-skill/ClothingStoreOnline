using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.ViewModels;

namespace ClothingStoreWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem Username hoặc Email đã tồn tại chưa
                var userExists = _context.Users.Any(u => u.Username == model.Username || u.Email == model.Email);
                if (userExists)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc Email đã tồn tại.");
                    return View(model);
                }

                // Tạo user mới, gán mặc định role Customer
                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = HashPassword(model.Password), // Mã hóa pass
                    Email = model.Email,
                    Role = "Customer"
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // --- ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = HashPassword(model.Password);
                var user = _context.Users.SingleOrDefault(u => u.Username == model.Username && u.PasswordHash == hashedPassword);

                if (user != null)
                {
                    // Tạo danh sách Claims (Thông tin lưu trong Cookie)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role), // Gán role vào Cookie
                        new Claim("UserId", user.UserID.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = true };

                    // Đăng nhập hệ thống
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Chuyển hướng dựa trên Role
                    if (user.Role == "Admin") return RedirectToAction("Index", "Admin");
                    if (user.Role == "Staff") return RedirectToAction("Index", "Staff");
                    return RedirectToAction("Index", "Home"); // Khách hàng
                }

                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu.");
            }
            return View(model);
        }

        // --- ĐĂNG XUẤT ---
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Hàm hỗ trợ mã hóa mật khẩu bằng SHA256 (bảo mật cơ bản)
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}