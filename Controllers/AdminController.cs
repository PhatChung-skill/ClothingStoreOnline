using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingStoreWeb.Controllers
{
    [Authorize(Roles = "Admin")] // Chặn, chỉ cho Admin vào
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return Content("CHÀO MỪNG ADMIN ĐẾN VỚI TRANG QUẢN TRỊ CAO NHẤT!");
        }
    }
}