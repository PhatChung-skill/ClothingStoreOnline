using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Guard against possible null references on User and Identity
        if (User?.Identity?.IsAuthenticated == true)
        {
            var name = User.Identity?.Name ?? "";
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            return Content($"CHÀO MỪNG {name} (Role: {role}) ĐÃ VÀO TRANG CHỦ MUA SẮM!");
        }

        return Content("CHÀO MỪNG KHÁCH VÃNG LAI! HÃY ĐĂNG NHẬP ĐỂ MUA SẮM.");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
