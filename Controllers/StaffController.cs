using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingStoreWeb.Controllers
{
    // Cấp quyền cho cả Admin và Staff đều có thể vào Workspace này
    [Authorize(Roles = "Admin,Staff")] 
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}