using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingStoreWeb.Controllers
{
    [Authorize(Roles = "Staff, Admin")] // Staff hoặc Admin đều vào được
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return Content("CHÀO MỪNG STAFF ĐẾN VỚI TRANG QUẢN LÝ ĐƠN HÀNG!");
        }
    }
}