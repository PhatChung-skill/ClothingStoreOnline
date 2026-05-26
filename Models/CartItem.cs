namespace ClothingStoreWeb.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public int VariantID { get; set; } // Trọng tâm để biết mua Size/Màu nào
        public string ProductName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal TotalPrice => Price * Quantity;
    }
}