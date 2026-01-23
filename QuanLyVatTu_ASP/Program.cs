using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Implementations;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using QuanLyVatTu_ASP.Services.Interfaces;
using QuanLyVatTu_ASP.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>();

// Đăng ký Repository
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


//Đăng ký services
builder.Services.AddScoped<INhanVienService, NhanVienService>();
builder.Services.AddScoped<IKhachHangService, KhachHangService>();
builder.Services.AddScoped<IDonHangService, DonHangService>();
builder.Services.AddScoped<IChiTietDonHangService, ChiTietDonHangService>();
builder.Services.AddScoped<IHoaDonService, HoaDonService>();
builder.Services.AddScoped<ILoaiVatTuService, LoaiVatTuService>();
builder.Services.AddScoped<INhaCungCapService, NhaCungCapService>();
builder.Services.AddScoped<IVatTuService, VatTuService>();
builder.Services.AddScoped<IThongKeService, ThongKeService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".QuanLyVatTu.Session";
});

builder.Services.AddHttpContextAccessor();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=DonHang}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();