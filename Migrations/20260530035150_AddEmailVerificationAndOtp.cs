using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClothingStoreWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationAndOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmailOtps",
                columns: table => new
                {
                    EmailOtpID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OtpHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOtps", x => x.EmailOtpID);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "EmailVerified",
                value: true);

            // Các tài khoản đã tồn tại trước khi bật xác thực email vẫn đăng nhập bình thường
            migrationBuilder.Sql("UPDATE Users SET EmailVerified = 1 WHERE EmailVerified = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailOtps");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Users");
        }
    }
}
