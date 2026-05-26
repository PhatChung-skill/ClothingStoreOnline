using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductImageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductImageController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // 1. HIỂN THỊ DANH SÁCH ẢNH CỦA 1 SẢN PHẨM
        public IActionResult Index(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            ViewBag.ProductName = product.Name;
            ViewBag.ProductId = productId;

            var images = _context.ProductImages.Where(i => i.ProductID == productId).ToList();
            return View(images);
        }

        // 2. XỬ LÝ UPLOAD ẢNH
        [HttpPost]
        public async Task<IActionResult> Upload(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Chưa chọn file.");

            // KIỂM TRA ĐIỀU KIỆN 3 ẢNH
            int currentImageCount = _context.ProductImages.Count(i => i.ProductID == productId);
            if (currentImageCount >= 3)
            {
                TempData["Error"] = "Sản phẩm này đã đạt tối đa 3 ảnh. Hãy xóa bớt trước khi thêm mới.";
                return RedirectToAction(nameof(Index), new { productId });
            }

            // XỬ LÝ LƯU FILE VẬT LÝ
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); // Tạo tên file duy nhất
            string productPath = Path.Combine(wwwRootPath, @"images\products");

            if (!Directory.Exists(productPath)) Directory.CreateDirectory(productPath);

            using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // LƯU VÀO DATABASE
            var productImage = new ProductImage
            {
                ProductID = productId,
                ImageUrl = @"/images/products/" + fileName,
                IsThumbnail = (currentImageCount == 0) // Nếu là ảnh đầu tiên thì gán làm Thumbnail luôn
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tải ảnh lên thành công!";
            return RedirectToAction(nameof(Index), new { productId });
        }

        // 3. XỬ LÝ XÓA ẢNH
        [HttpPost]
        public IActionResult Delete(int imageId)
        {
            var image = _context.ProductImages.Find(imageId);
            if (image == null) return NotFound();

            // Xóa file vật lý
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            int pid = image.ProductID;
            _context.ProductImages.Remove(image);
            _context.SaveChanges();

            TempData["Success"] = "Đã xóa ảnh.";
            return RedirectToAction(nameof(Index), new { productId = pid });
        }
    }
}