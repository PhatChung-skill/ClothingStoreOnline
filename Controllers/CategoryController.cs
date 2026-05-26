using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Controllers
{
    [Authorize(Roles = "Admin")] // Bắt buộc phải là Admin mới được vào
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH (Read)
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // 2. THÊM MỚI (Create)
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["Success"] = "Đã thêm danh mục mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 3. CHỈNH SỬA (Update)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                TempData["Success"] = "Đã cập nhật danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 4. XÓA (Delete)
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
                TempData["Success"] = "Đã xóa danh mục thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}