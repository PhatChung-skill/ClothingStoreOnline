using System.ComponentModel.DataAnnotations;

namespace ClothingStoreWeb.Models
{
    public class EmailOtp
    {
        [Key]
        public int EmailOtpID { get; set; }

        [Required]
        [StringLength(100)]
        public required string Email { get; set; }

        [Required]
        [StringLength(64)]
        public required string OtpHash { get; set; }

        /// <summary>Register | ForgotPassword</summary>
        [Required]
        [StringLength(30)]
        public required string Purpose { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public static class OtpPurpose
    {
        public const string Register = "Register";
        public const string ForgotPassword = "ForgotPassword";
    }
}
