using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingStoreWeb.Models
{
    public class ProductVariant
    {
        [Key]
        public int VariantID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Màu sắc không được để trống")]
        [StringLength(50)]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kích cỡ không được để trống")]
        [StringLength(10)]
        public string Size { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số lượng tồn kho không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không thể âm")]
        public int StockQuantity { get; set; }

        // Khóa ngoại liên kết ngược lại Sản phẩm
        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }

        // Liên kết tới chi tiết đơn hàng
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}