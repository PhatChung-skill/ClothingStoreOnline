using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào trang này
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH ĐƠN HÀNG
        public IActionResult Index()
        {
            // Lấy tất cả đơn hàng, kèm theo thông tin người dùng (User)
            var orders = _context.Orders
                                 .Include(o => o.User)
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToList();
            return View(orders);
        }

        // 2. XEM CHI TIẾT ĐƠN HÀNG (OrderDetail)
        public IActionResult Details(int id)
        {
            var order = _context.Orders
                                .Include(o => o.User)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(od => od.ProductVariant)
                                        .ThenInclude(v => v!.Product)
                                .FirstOrDefault(o => o.OrderID == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // 3. CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
        [HttpPost]
        public IActionResult UpdateStatus(int orderId, string status)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
                TempData["Success"] = "Đã cập nhật trạng thái đơn hàng thành công!";
            }
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}