using System.ComponentModel.DataAnnotations;

namespace ClothingStoreWeb.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Quan hệ 1 - Nhiều: Một danh mục chứa nhiều Sản phẩm
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}