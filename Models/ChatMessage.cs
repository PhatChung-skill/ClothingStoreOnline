using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingStoreWeb.Models
{
    public class ChatMessage
    {
        [Key]
        public int MessageID { get; set; }

        [Required]
        public int UserID { get; set; } // Liên kết với Khách hàng sở hữu cuộc hội thoại này

        [Required]
        public string MessageText { get; set; } = string.Empty;

        // true = Nhân viên gửi | false = Khách hàng gửi
        public bool IsFromStaff { get; set; } = false; 

        public DateTime SentAt { get; set; } = DateTime.Now;

        // Dùng để Staff biết tin nhắn này đã đọc hay chưa
        public bool IsRead { get; set; } = false; 

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
    }
}