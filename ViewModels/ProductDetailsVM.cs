using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.ViewModels
{
    public class ProductDetailsVM
    {
        public Product Product { get; set; } = new Product();
        public List<string> AvailableColors { get; set; } = new List<string>();
        public List<string> AvailableSizes { get; set; } = new List<string>();
        
        // Dùng để lưu lựa chọn của khách khi bấm "Thêm vào giỏ"
        public string SelectedColor { get; set; } = string.Empty;
        public string SelectedSize { get; set; } = string.Empty;
    }
}