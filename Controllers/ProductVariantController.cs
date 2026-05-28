using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductVariantController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductVariantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH BIẾN THỂ CỦA 1 SẢN PHẨM
        public async Task<IActionResult> Index(int productId, string? search = null, int page = 1)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;

            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductID == productId);
            if (product == null) return NotFound();

            ViewBag.ProductName = product.Name;
            ViewBag.ProductId = productId;

            IQueryable<ProductVariant> query = _context.ProductVariants.Where(v => v.ProductID == productId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var cleanSearch = search.Trim().ToLower();
                query = query.Where(v => v.Color.ToLower().Contains(cleanSearch)
                                      || v.Size.ToLower().Contains(cleanSearch));
            }

            query = query.OrderByDescending(v => v.VariantID);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var variants = await query.AsNoTracking()
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            return View(variants);
        }

        // 2. THÊM BIẾN THỂ MỚI
        [HttpGet]
        public IActionResult Create(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            ViewBag.ProductName = product.Name;
            return View(new ProductVariant { ProductID = productId });
        }

        [HttpPost]
        public IActionResult Create(ProductVariant variant)
        {
            if (ModelState.IsValid)
            {
                _context.ProductVariants.Add(variant);
                _context.SaveChanges();
                TempData["Success"] = "Đã thêm biến thể mới!";
                return RedirectToAction(nameof(Index), new { productId = variant.ProductID });
            }
            return View(variant);
        }

        // 3. CẬP NHẬT TỒN KHO/SIZE/MÀU
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var variant = _context.ProductVariants.Include(v => v.Product).FirstOrDefault(v => v.VariantID == id);
            if (variant == null || variant.Product == null) return NotFound();

            ViewBag.ProductName = variant.Product.Name;
            return View(variant);
        }

        [HttpPost]
        public IActionResult Edit(ProductVariant variant)
        {
            if (ModelState.IsValid)
            {
                _context.ProductVariants.Update(variant);
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật tồn kho thành công!";
                return RedirectToAction(nameof(Index), new { productId = variant.ProductID });
            }
            return View(variant);
        }

        // 4. XÓA BIẾN THỂ
        [HttpPost]
        public IActionResult Delete(int variantId)
        {
            var variant = _context.ProductVariants.Find(variantId);
            if (variant == null) return NotFound();

            int pid = variant.ProductID;
            _context.ProductVariants.Remove(variant);
            _context.SaveChanges();

            TempData["Success"] = "Đã xóa biến thể.";
            return RedirectToAction(nameof(Index), new { productId = pid });
        }
    }
}