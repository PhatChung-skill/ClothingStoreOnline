using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.ViewModels;

namespace ClothingStoreWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH SẢN PHẨM
        public IActionResult Index()
        {
            // Kết nối bảng Category để lấy Name
            var products = _context.Products.Include(p => p.Category).ToList();
            return View(products);
        }

        // 2. THÊM SẢN PHẨM MỚI
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new ProductVM
            {
                Name = string.Empty,
                Description = string.Empty,
                BasePrice = 0,
                CategoryList = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryID.ToString(),
                    Text = c.Name
                })
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(ProductVM vm)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    CategoryID = vm.CategoryID,
                    Name = vm.Name ?? string.Empty,
                    Description = vm.Description ?? string.Empty,
                    BasePrice = vm.BasePrice
                };

                _context.Products.Add(product);
                _context.SaveChanges();
                TempData["Success"] = "Đã thêm sản phẩm gốc thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại danh sách Category
            vm.CategoryList = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.CategoryID.ToString(),
                Text = c.Name
            });
            return View(vm);
        }

        // 3. CẬP NHẬT SẢN PHẨM
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            var vm = new ProductVM
            {
                ProductID = product.ProductID,
                CategoryID = product.CategoryID,
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                CategoryList = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryID.ToString(),
                    Text = c.Name
                })
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(ProductVM vm)
        {
            if (ModelState.IsValid)
            {
                var product = _context.Products.Find(vm.ProductID);
                if (product != null)
                {
                    product.CategoryID = vm.CategoryID;
                    product.Name = vm.Name ?? string.Empty;
                    product.Description = vm.Description ?? string.Empty;
                    product.BasePrice = vm.BasePrice;

                    _context.Products.Update(product);
                    _context.SaveChanges();
                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                }
                return RedirectToAction(nameof(Index));
            }

            vm.CategoryList = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.CategoryID.ToString(),
                Text = c.Name
            });
            return View(vm);
        }

        // 4. XÓA SẢN PHẨM
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                TempData["Success"] = "Đã xóa sản phẩm thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}