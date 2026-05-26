using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.ViewModels;
using ClothingStoreWeb.Data; 

namespace ClothingStoreWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    
    private readonly ApplicationDbContext _context; 

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        // Lấy danh sách sản phẩm, kèm theo hình ảnh để làm hiệu ứng Hover
        // Dùng thẳng _context.Products thay vì _context.Set<Product>() cho gọn gàng
        var products = _context.Products
                               .Include(p => p.ProductImages)
                               .ToList();
        return View(products);
    }

    public IActionResult Details(int id)
    {
        // Lấy sản phẩm kèm ảnh và các biến thể tồn kho
        var product = _context.Products
                            .Include(p => p.ProductImages)
                            .Include(p => p.ProductVariants)
                            .Include(p => p.Category)
                            .FirstOrDefault(p => p.ProductID == id);

        if (product == null) return NotFound();

        // Lấy danh sách các màu và size duy nhất đang có của sản phẩm này
        var viewModel = new ProductDetailsVM
        {
            Product = product,
            AvailableColors = product.ProductVariants.Select(v => v.Color).Distinct().ToList(),
            AvailableSizes = product.ProductVariants.Select(v => v.Size).Distinct().ToList()
        };

        return View(viewModel);
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