using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để chat
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private DbSet<ChatMessage> ChatMessages => _context.Set<ChatMessage>();

        public ChatController(ApplicationDbContext context) { _context = context; }

        // ==========================================
        // 1. PHÂN HỆ DÀNH CHO KHÁCH HÀNG
        // ==========================================
        public IActionResult Index()
        {
            var username = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound();

            var messages = ChatMessages
                                   .Where(m => m.UserID == user.UserID)
                                   .OrderBy(m => m.SentAt)
                                   .ToList();
            return View(messages);
        }

        [HttpPost]
        public IActionResult Send(string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText)) return RedirectToAction(nameof(Index));

            var username = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            var chat = new ChatMessage 
            { 
                UserID = user!.UserID, 
                MessageText = messageText, 
                IsFromStaff = false 
            };
            
            ChatMessages.Add(chat);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 2. PHÂN HỆ DÀNH CHO NHÂN VIÊN (STAFF/ADMIN)
        // ==========================================
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult StaffList()
        {
            // Lấy danh sách khách hàng ĐÃ TỪNG NHẮN TIN, kèm theo tin nhắn cuối và số tin chưa đọc
            var usersWithChats = _context.Users
                .Where(u => ChatMessages.Any(m => m.UserID == u.UserID))
                .Select(u => new 
                {
                    User = u,
                    LastMessage = ChatMessages.Where(m => m.UserID == u.UserID).OrderByDescending(m => m.SentAt).FirstOrDefault(),
                    UnreadCount = ChatMessages.Count(m => m.UserID == u.UserID && !m.IsFromStaff && !m.IsRead)
                })
                .OrderByDescending(x => x.LastMessage!.SentAt)
                .ToList();

            // Gửi dữ liệu qua ViewBag vì đang dùng dynamic object
            ViewBag.ChatList = usersWithChats;
            return View();
        }

        [Authorize(Roles = "Admin,Staff")]
        public IActionResult StaffDetails(int id)
        {
            var messages = ChatMessages
                                   .Include(m => m.User)
                                   .Where(m => m.UserID == id)
                                   .OrderBy(m => m.SentAt)
                                   .ToList();

            // Tự động đánh dấu "Đã đọc" khi nhân viên click vào xem
            var unread = messages.Where(m => !m.IsFromStaff && !m.IsRead).ToList();
            if (unread.Any())
            {
                unread.ForEach(m => m.IsRead = true);
                _context.SaveChanges();
            }

            ViewBag.Customer = _context.Users.Find(id);
            return View(messages);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult StaffReply(int userId, string messageText)
        {
            if (!string.IsNullOrWhiteSpace(messageText))
            {
                var chat = new ChatMessage 
                { 
                    UserID = userId, 
                    MessageText = messageText, 
                    IsFromStaff = true, 
                    IsRead = true // Tin nhân viên gửi thì mặc định không tính là tin rác chưa đọc
                };
                ChatMessages.Add(chat);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(StaffDetails), new { id = userId });
        }
    }
}