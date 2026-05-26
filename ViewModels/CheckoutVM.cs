using System.ComponentModel.DataAnnotations;

namespace ClothingStoreWeb.ViewModels
{
    public class CheckoutVM
    {
        [Required(ErrorMessage = "Họ và tên người nhận không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại nhận hàng không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ nhận hàng không được để trống")]
        [StringLength(255)]
        public string ShippingAddress { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "COD"; // Mặc định là nhận hàng thanh toán tiền
    }
}