using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Services;
using ClothingStoreWeb.ViewModels;

namespace ClothingStoreWeb.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Auth");
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Auth");

            return View(new ChangePasswordVM
            {
                Username = user.Username,
                Email = user.Email
            });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Auth");

            if (!string.Equals(user.Username, model.Username, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(user.Email, model.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc email không khớp với tài khoản hiện tại.");
                return View(model);
            }

            if (user.PasswordHash != PasswordHelper.Hash(model.CurrentPassword))
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            if (model.CurrentPassword == model.NewPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới phải khác mật khẩu hiện tại.");
                return View(model);
            }

            user.PasswordHash = PasswordHelper.Hash(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Profile));
        }

        private async Task<Models.User?> GetCurrentUserAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return null;

            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
