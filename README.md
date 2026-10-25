# Nhất Thanh: hệ thống quản lý khách sạn

Ứng dụng web quản lý khách sạn viết bằng ASP.NET Core Razor Pages (.NET 10), lưu dữ liệu trong MySQL qua Entity Framework Core.

Chức năng chính: đăng nhập quản trị, quản lý phòng và loại phòng, đặt phòng, khách hàng, nhân viên, báo cáo, và thanh toán đặt phòng qua cổng VNPAY (môi trường sandbox).

## Công nghệ

ASP.NET Core Razor Pages trên .NET 10, Entity Framework Core với Pomelo.EntityFrameworkCore.MySql 9.0.0, MySQL 8 trở lên, session để lưu trạng thái đăng nhập, VNPAY sandbox cho thanh toán.

## Yêu cầu cài đặt trước

1. .NET SDK 10. Kiểm tra bằng lệnh `dotnet --version`, kết quả phải bắt đầu bằng `10.`
2. MySQL Server 8 trở lên, chạy ở cổng 3306.
3. Git.
4. ngrok, chỉ cần khi muốn thử thanh toán VNPAY (xem bước 6).

## Chạy dự án

### Bước 1. Lấy mã nguồn

```bash
git clone https://github.com/Tizkan/nhom1dotnet.git
cd nhom1dotnet
```

### Bước 2. Tạo cơ sở dữ liệu

File `Database/hotelmanager.sql` chứa cấu trúc bảng và dữ liệu mẫu nhưng không có lệnh tạo database, nên phải tạo database trước rồi mới nạp file.

```bash
mysql -u root -p -e "CREATE DATABASE hotelmanager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
mysql -u root -p hotelmanager < Database/hotelmanager.sql
```

Trên PowerShell, toán tử `<` không dùng được. Dùng lệnh này thay thế:

```powershell
Get-Content Database/hotelmanager.sql -Encoding UTF8 | mysql -u root -p hotelmanager
```

Nếu dùng MySQL Workbench: tạo schema `hotelmanager`, chọn Server > Data Import > Import from Self-Contained File, trỏ đến `Database/hotelmanager.sql`, chọn Default Target Schema là `hotelmanager`, bấm Start Import.

Sau khi nạp xong, database có 9 bảng: `booking_services`, `bookings`, `customers`, `payments`, `room_types`, `rooms`, `services`, `staffs` và `users`.

### Bước 3. Sửa chuỗi kết nối

Mở `appsettings.json` và đổi `user`, `password` cho khớp MySQL trên máy bạn:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=hotelmanager;user=root;password=MAT_KHAU_MYSQL_CUA_BAN"
}
```

Mật khẩu mặc định trong repo là `123456`. Nếu MySQL của bạn đặt mật khẩu khác thì bắt buộc phải sửa, nếu không ứng dụng báo lỗi `Access denied` ngay lần truy vấn đầu tiên.

### Bước 4. Khôi phục package và chạy

```bash
dotnet restore
dotnet run --launch-profile https
```

Khi console hiện dòng `Now listening on: https://localhost:7100`, mở trình duyệt vào địa chỉ đó. Profile `http` chạy ở `http://localhost:5153`:

```bash
dotnet run --launch-profile http
```

Lần đầu chạy bằng HTTPS, trình duyệt có thể cảnh báo chứng chỉ. Chạy lệnh sau một lần để tin cậy chứng chỉ dev:

```bash
dotnet dev-certs https --trust
```

### Bước 5. Đăng nhập

Tài khoản mẫu có sẵn trong file SQL:

```text
Email:    admin@gmail.com
Mật khẩu: 123456
```

Nhập email vào ô tài khoản. Đăng nhập đúng thì hệ thống chuyển sang trang tổng quan.

### Bước 6. Thử thanh toán VNPAY (không bắt buộc)

VNPAY gọi ngược về ứng dụng sau khi thanh toán, nên `ReturnUrl` phải là địa chỉ mà internet truy cập được. Localhost không dùng được, vì vậy cần ngrok.

1. Chạy ứng dụng ở profile `http` (cổng 5153).
2. Mở terminal khác và chạy:

   ```bash
   ngrok http 5153
   ```

3. Copy địa chỉ `https://....ngrok-free.dev` mà ngrok in ra.
4. Sửa mục `Vnpay` trong `appsettings.json`:

   ```json
   "Vnpay": {
     "TmnCode": "MÃ_TMN_CỦA_BẠN",
     "HashSecret": "HASH_SECRET_CỦA_BẠN",
     "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
     "ReturnUrl": "https://DIA_CHI_NGROK_CUA_BAN/Booking/VnpayReturn"
   }
   ```

5. Chạy lại `dotnet run --launch-profile http`, vào Booking, tạo đặt phòng và bấm thanh toán. Trang VNPAY sandbox mở ra, dùng thẻ test trong tài liệu sandbox của VNPAY để hoàn tất.

Địa chỉ ngrok bản miễn phí đổi sau mỗi lần chạy lại ngrok, nên bước 4 phải làm lại mỗi lần.

## Cấu trúc thư mục

```text
nhom1dotnet/
├── Data/            AppDbContext.cs, ánh xạ bảng MySQL
├── Database/        hotelmanager.sql, file tạo bảng và dữ liệu mẫu
├── Models/          Bookings, Customer, Payment, Room, RoomType, Staff, User
├── Pages/
│   ├── Booking/     danh sách, tạo, sửa đặt phòng, nhận kết quả VNPAY
│   ├── Customers/   khách hàng
│   ├── Login/       đăng nhập
│   ├── Logout/      đăng xuất
│   ├── Report/      báo cáo
│   ├── Rooms/       danh sách, tạo, sửa phòng
│   ├── Services/    VnPayService.cs, tạo URL thanh toán
│   ├── Settings/    cài đặt
│   ├── Shared/      _Layout.cshtml, thanh điều hướng bên trái
│   └── Staff/       nhân viên
├── wwwroot/         css, js, lib
├── Program.cs       cấu hình dịch vụ, session, kết nối MySQL
└── appsettings.json chuỗi kết nối và cấu hình VNPAY
```

## Lỗi thường gặp

`Unable to connect to any of the specified MySQL hosts`: MySQL chưa chạy hoặc sai cổng. Kiểm tra dịch vụ MySQL đang chạy và cổng trong chuỗi kết nối.

`Unknown database 'hotelmanager'`: chưa làm bước 2. Tạo database rồi nạp lại file SQL.

`Access denied for user 'root'`: mật khẩu trong `appsettings.json` khác mật khẩu MySQL thật. Sửa lại ở bước 3.

`dotnet restore` lỗi không tải được package: xóa cache NuGet rồi khôi phục lại.

```bash
dotnet nuget locals all --clear
dotnet restore
```

`A compatible .NET SDK was not found`: máy chưa cài .NET 10. Tải SDK 10 từ dotnet.microsoft.com rồi kiểm tra lại bằng `dotnet --version`.

Thanh toán VNPAY xong nhưng không quay về được ứng dụng: `ReturnUrl` đang trỏ đến địa chỉ ngrok cũ. Làm lại bước 6.

## Nhóm thực hiện

Nhóm 1. Mã nguồn: https://github.com/Tizkan/nhom1dotnet****
