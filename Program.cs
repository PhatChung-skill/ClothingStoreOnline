using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.ResponseCompression;
using ClothingStoreWeb.Data;
using ClothingStoreWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext — ổn định hơn DbContextPool trên SQL Server Express local
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.CommandTimeout(60)));

// Cấu hình Authentication bằng Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Nếu chưa đăng nhập sẽ chuyển tới đây
        options.LoginPath = "/Auth/Login";

        // Nếu không đủ quyền sẽ chuyển tới đây
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Giỏ hàng tự hủy sau 30 phút không thao tác
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

// Phục vụ file tĩnh: css, js, images...
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 7; // 7 ngày
        context.Context.Response.Headers.CacheControl = $"public,max-age={durationInSeconds}";
    }
});

app.UseRouting();

// Kích hoạt Authentication
app.UseAuthentication();

app.UseSession();

// Kích hoạt Authorization
app.UseAuthorization();

// Trang 404 tùy chỉnh khi URL không tồn tại hoặc action trả về NotFound()
app.UseStatusCodePagesWithReExecute("/Home/NotFoundPage", "?statusCode={0}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();