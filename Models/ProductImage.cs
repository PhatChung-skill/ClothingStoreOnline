using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingStoreWeb.Models
{
    public class ProductImage
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        [StringLength(255)]
        public required string ImageUrl { get; set; }

        [Required]
        public bool IsThumbnail { get; set; } = false; // Đánh dấu ảnh đại diện chính hiển thị ở trang chủ

        // Khóa ngoại liên kết tới Sản phẩm
        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }
    }
}