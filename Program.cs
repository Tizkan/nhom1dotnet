using Microsoft.EntityFrameworkCore;
using nhom1dotnet.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); //lấy chuỗi kết nối từ

builder.Services.AddRazorPages();
builder.Services.AddScoped<VnPayService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)//EF tự phát hiện phiên bản MySQL/MariaDB đang dùng (8.0, 10.x,...)
    )
);


var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();