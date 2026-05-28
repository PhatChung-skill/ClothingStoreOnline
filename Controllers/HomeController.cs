using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.ViewModels;
using ClothingStoreWeb.Data; 
using ClothingStoreWeb;

namespace ClothingStoreWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private const string CartSessionKey = "ClothingCart";
    
    private readonly ApplicationDbContext _context; 

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string? search = null, int page = 1)
    {
        const int pageSize = 9; // 9 items per page fits the 3-column responsive grid perfectly
        if (page < 1) page = 1;

        IQueryable<Product> query = _context.Products.Include(p => p.ProductImages).Include(p => p.Category);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchClean = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchClean) 
                                  || p.Description.ToLower().Contains(searchClean)
                                  || (p.Category != null && p.Category.Name.ToLower().Contains(searchClean)));
        }

        query = query.OrderByDescending(p => p.ProductID);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (totalPages == 0) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var products = await query.AsNoTracking()
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

        var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        ViewBag.CartPreview = cartItems.Take(3).ToList();
        ViewBag.CartCount = cartItems.Sum(i => i.Quantity);
        ViewBag.CartTotal = cartItems.Sum(i => i.TotalPrice);
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Search = search;

        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        // Lấy sản phẩm kèm ảnh và các biến thể tồn kho
        var product = await _context.Products
                                    .AsNoTracking()
                                    .Include(p => p.ProductImages)
                                    .Include(p => p.ProductVariants)
                                    .Include(p => p.Category)
                                    .FirstOrDefaultAsync(p => p.ProductID == id);

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