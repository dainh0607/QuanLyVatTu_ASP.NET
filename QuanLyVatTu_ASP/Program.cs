using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Implementations;
using QuanLyVatTu_ASP.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QuanLyVatTuDB")));

// Đăng ký Dependency Injection cho Repository + UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IVatTuRepository, VatTuRepository>();
builder.Services.AddScoped<ILoaiVatTuRepository, LoaiVatTuRepository>();
builder.Services.AddScoped<INhaCungCapRepository, NhaCungCapRepository>();
builder.Services.AddScoped<IKhachHangRepository, KhachHangRepository>();
builder.Services.AddScoped<INhanVienRepository, NhanVienRepository>();
builder.Services.AddScoped<IDonHangRepository, DonHangRepository>();
builder.Services.AddScoped<IChiTietDonHangRepository, ChiTietDonHangRepository>();
builder.Services.AddScoped<IHoaDonRepository, HoaDonRepository>();
builder.Services.AddScoped<IChiTietHoaDonRepository, ChiTietHoaDonRepository>();

builder.Services.AddHttpContextAccessor();

// ❗❗❗ TẤT CẢ AddScoped/AddDbContext PHẢI ĐỨNG TRƯỚC builder.Build()
var app = builder.Build();

// Kiểm tra lấy connection string từ appsettings.json
string connectionString = builder.Configuration.GetConnectionString("QuanLyVatTuDB")
    ?? throw new InvalidOperationException("Không tìm thấy ConnectionString 'QuanLyVatTuDB' trong appsettings.json");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Route cho các Area
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=DonHang}/{action=Index}/{id?}");

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customer}/{action=Index}/{id?}");

app.Run();
