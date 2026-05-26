using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClothingStoreWeb.ViewModels
{
    public class ProductVM
    {
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(200)]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal BasePrice { get; set; }

        // Danh sách danh mục để đổ vào thẻ <select>
        public IEnumerable<SelectListItem>? CategoryList { get; set; }
    }
}