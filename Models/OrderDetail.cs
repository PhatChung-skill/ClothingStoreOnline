using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingStoreWeb.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [Required]
        public int VariantID { get; set; } // Liên kết trực tiếp tới biến thể để biết rõ khách mua Size/Màu nào

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng mua tối thiểu là 1")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Lưu giá bán tại thời điểm mua (đề phòng sản phẩm đổi giá gốc sau này)

        // Khóa ngoại liên kết tới Đơn hàng tổng
        [ForeignKey("OrderID")]
        public virtual Order? Order { get; set; }

        // Khóa ngoại liên kết tới Biến thể sản phẩm
        [ForeignKey("VariantID")]
        public virtual ProductVariant? ProductVariant { get; set; }
    }
}