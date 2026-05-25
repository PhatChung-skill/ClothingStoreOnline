using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingStoreWeb.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Định dạng tiền tệ chuẩn trong SQL Server
        public decimal BasePrice { get; set; }

        // Khóa ngoại liên kết tới Danh mục
        [ForeignKey("CategoryID")]
        public virtual Category? Category { get; set; }

        // Quan hệ 1 - Nhiều: Một sản phẩm có nhiều biến thể tồn kho
        public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

        // Quan hệ 1 - Nhiều: Một sản phẩm có tối đa 3 ảnh (điều kiện số lượng kiểm tra bằng code Backend sau này)
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}