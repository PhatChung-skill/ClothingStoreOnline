using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ duy nhất quyền Admin tối cao mới được truy cập
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH TÀI KHOẢN
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // 2. XỬ LÝ KHÓA / MỞ KHÓA TÀI KHOẢN
        [HttpPost]
        public IActionResult ToggleLock(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            // Quy tắc bảo mật: Không được tự khóa chính mình
            if (user.Username == User.Identity?.Name)
            {
                TempData["Error"] = "Hành động bị từ chối: Bạn không thể tự khóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            user.IsLocked = !user.IsLocked;
            _context.SaveChanges();

            TempData["Success"] = user.IsLocked 
                ? $"Đã khóa thành công tài khoản: {user.Username}" 
                : $"Đã mở khóa thành công tài khoản: {user.Username}";

            return RedirectToAction(nameof(Index));
        }

        // 3. XỬ LÝ THAY ĐỔI QUYỀN HẠN (ROLE)
        [HttpPost]
        public IActionResult ChangeRole(int id, string newRole)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            // Quy tắc bảo mật: Không được tự hạ quyền Admin của mình xuống quyền khác
            if (user.Username == User.Identity?.Name)
            {
                TempData["Error"] = "Hành động bị từ chối: Bạn không thể tự thay đổi quyền hạn của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            user.Role = newRole;
            _context.SaveChanges();

            TempData["Success"] = $"Đã chuyển đổi quyền của tài khoản {user.Username} sang: {newRole}";
            return RedirectToAction(nameof(Index));
        }
    }
}