using System.ComponentModel.DataAnnotations;

namespace ClothingStoreWeb.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự")]
        public required string Username { get; set; }

        [Required]
        [StringLength(255)]
        public required string PasswordHash { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public required string Email { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string Role { get; set; } = "Customer"; // Mặc định là Khách hàng

        // Quan hệ 1 - Nhiều: Một người dùng có thể có nhiều Đơn hàng
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}