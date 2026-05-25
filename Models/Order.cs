using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingStoreWeb.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public int? UserID { get; set; } // Nullable để hỗ trợ khách vãng lai mua nhanh

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Các trạng thái: Pending, Shipping, Completed, Cancelled

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "COD"; // Phương thức: COD hoặc QR_Transfer

        [Required(ErrorMessage = "Địa chỉ nhận hàng không được để trống")]
        [StringLength(255)]
        public string ShippingAddress { get; set; } = string.Empty;

        // Khóa ngoại liên kết tới người mua
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        // Chi tiết danh sách sản phẩm trong đơn hàng này
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}