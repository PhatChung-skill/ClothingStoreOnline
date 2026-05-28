using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Chèn sẵn dữ liệu Danh mục (Categories)
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, Name = "Áo Thun" },
                new Category { CategoryID = 2, Name = "Quần Jean" },
                new Category { CategoryID = 3, Name = "Áo Khoác" }
            );

            // 2. Chèn sẵn 1 tài khoản Admin để đăng nhập kiểm thử
            // Lưu ý: Trong thực tế mật khẩu phải được băm (Hash), ở đây để demo tạm chuỗi thô nhé
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    UserID = 1, 
                    Username = "admin", 
                    PasswordHash = "admin123", 
                    Email = "admin@clothingstore.com",
                    FullName = "Hệ Thống Quản Trị",
                    Phone = "0123456789",
                    Address = "TP HCM",
                    Role = "Admin" 
                }
            );
        }
    }
}