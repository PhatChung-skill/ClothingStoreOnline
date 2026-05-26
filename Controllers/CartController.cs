using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.ViewModels;

namespace ClothingStoreWeb.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới sử dụng được Giỏ hàng theo yêu cầu của bạn
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CART_SESSION_KEY = "ClothingCart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng hiện tại ra
        private List<CartItem> GetCartItems()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>(CART_SESSION_KEY) ?? new List<CartItem>();
        }

        // 1. TRANG DANH SÁCH GIỎ HÀNG
        public IActionResult Index()
        {
            var cart = GetCartItems();
            ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
            return View(cart);
        }

        // 2. THÊM SẢN PHẨM VÀO GIỎ HÀNG
        [HttpPost]
        public IActionResult Add(int productId, string selectedColor, string selectedSize)
        {
            if (string.IsNullOrEmpty(selectedColor) || string.IsNullOrEmpty(selectedSize))
            {
                TempData["Error"] = "Vui lòng chọn đầy đủ Màu sắc và Kích cỡ!";
                return RedirectToAction("Details", "Home", new { id = productId });
            }

            // Tìm chính xác biến thể trong kho
            var variant = _context.ProductVariants
                                  .Include(v => v.Product)
                                  .FirstOrDefault(v => v.ProductID == productId && v.Color == selectedColor && v.Size == selectedSize);

            if (variant == null || variant.StockQuantity <= 0)
            {
                TempData["Error"] = "Biến thể sản phẩm này hiện đã hết hàng!";
                return RedirectToAction("Details", "Home", new { id = productId });
            }

            // Lấy ảnh đại diện sản phẩm
            var mainImage = _context.ProductImages.FirstOrDefault(i => i.ProductID == productId)?.ImageUrl ?? "/images/no-image.png";

            var cart = GetCartItems();
            var cartItem = cart.FirstOrDefault(c => c.VariantID == variant.VariantID);

            if (cartItem != null)
            {
                cartItem.Quantity++; // Nếu trùng khít món đồ thì cộng dồn số lượng
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductID = productId,
                    VariantID = variant.VariantID,
                    ProductName = variant.Product?.Name ?? string.Empty,
                    Color = selectedColor,
                    Size = selectedSize,
                    Price = variant.Product?.BasePrice ?? 0,
                    Quantity = 1,
                    ImageUrl = mainImage
                });
            }

            HttpContext.Session.SetObjectAsJson(CART_SESSION_KEY, cart);
            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // 3. XÓA MÓN ĐỒ KHỎI GIỎ HÀNG
        public IActionResult Remove(int variantId)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.VariantID == variantId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson(CART_SESSION_KEY, cart);
            }
            return RedirectToAction(nameof(Index));
        }

        
       // 4. TRANG TIẾN HÀNH THANH TOÁN (GET)
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCartItems();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            // Lấy tên đăng nhập hiện tại từ Claims
            var username = User.Identity?.Name;
            
            // Tìm User trong DB dựa trên cả Username hoặc Email (đề phòng lưu Claim kiểu khác nhau)
            var user = _context.Users.FirstOrDefault(u => u.Username == username || u.Email == username);

            var checkoutVm = new CheckoutVM();
            if (user != null)
            {
                // Ưu tiên lấy FullName, nếu null thì lấy temporary là Username để không bao giờ bị trống
                checkoutVm.FullName = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Username;
                checkoutVm.Phone = user.Phone ?? string.Empty;
                checkoutVm.ShippingAddress = user.Address ?? string.Empty;
            }

            ViewBag.Cart = cart;
            ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
            return View(checkoutVm);
        }

        // 5. XỬ LÝ ĐẶT HÀNG CHỐT ĐƠN (POST)
        [HttpPost]
        public IActionResult Checkout(CheckoutVM vm)
        {
            var cart = GetCartItems();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            if (ModelState.IsValid)
            {
                var username = User.Identity?.Name;
                var user = _context.Users.FirstOrDefault(u => u.Username == username || u.Email == username);

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // CẬP NHẬT LẠI THÔNG TIN USER NẾU TRONG DB ĐANG TRỐNG
                        if (user != null)
                        {
                            if (string.IsNullOrEmpty(user.FullName) || user.FullName == user.Username) 
                                user.FullName = vm.FullName;
                            if (string.IsNullOrEmpty(user.Phone)) user.Phone = vm.Phone;
                            if (string.IsNullOrEmpty(user.Address)) user.Address = vm.ShippingAddress;
                            
                            _context.Users.Update(user);
                        }

                        // A. Tạo đối tượng Order tổng quát
                        var order = new Order
                        {
                            UserID = user?.UserID,
                            OrderDate = DateTime.Now,
                            TotalAmount = cart.Sum(item => item.TotalPrice),
                            Status = "Pending",
                            PaymentMethod = vm.PaymentMethod,
                            ShippingAddress = vm.ShippingAddress // Địa chỉ giao hàng thực tế của đơn này
                        };

                        _context.Orders.Add(order);
                        _context.SaveChanges(); 

                        // B. Duyệt qua từng món trong giỏ để viết vào OrderDetail và TRỪ KHO
                        foreach (var item in cart)
                        {
                            var variant = _context.ProductVariants.Find(item.VariantID);
                            if (variant == null || variant.StockQuantity < item.Quantity)
                            {
                                // Bắn lỗi ngay lập tức nếu kho không đủ đáp ứng
                                throw new Exception($"Sản phẩm '{item.ProductName}' (Màu: {item.Color} / Size: {item.Size}) chỉ còn tồn {variant?.StockQuantity ?? 0} sản phẩm. Vui lòng giảm số lượng.");
                            }

                            variant.StockQuantity -= item.Quantity;

                            var orderDetail = new OrderDetail
                            {
                                OrderID = order.OrderID,
                                VariantID = item.VariantID,
                                Quantity = item.Quantity,
                                UnitPrice = item.Price
                            };
                            _context.OrderDetails.Add(orderDetail);
                        }

                        _context.SaveChanges();
                        transaction.Commit(); 

                        HttpContext.Session.Remove(CART_SESSION_KEY);
                        return RedirectToAction(nameof(OrderSuccess), new { orderId = order.OrderID });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", ex.Message);
                    }
                }
            }

            ViewBag.Cart = cart;
            ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
            return View(vm);
        }

        // Trang thông báo đặt hàng thành công
        public IActionResult OrderSuccess(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }
    }
}