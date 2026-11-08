# Nhất Thanh: hệ thống quản lý khách sạn

Ứng dụng web quản lý khách sạn viết bằng ASP.NET Core Razor Pages (.NET 10), lưu dữ liệu trong MySQL qua Entity Framework Core.

Chức năng chính: đăng nhập quản trị, quản lý phòng và loại phòng, đặt phòng, khách hàng, nhân viên, báo cáo, và thanh toán đặt phòng qua cổng VNPAY (môi trường sandbox).

## Công nghệ

- ASP.NET Core Razor Pages, .NET 10
- Entity Framework Core với Pomelo.EntityFrameworkCore.MySql 9.0.0
- MySQL 8 trở lên
- Session để lưu trạng thái đăng nhập
- VNPAY sandbox cho thanh toán

## Chạy nhanh (đã cài đủ công cụ)

```bash
git clone https://github.com/Tizkan/nhom1dotnet.git
cd nhom1dotnet
mysql -u root -p -e "CREATE DATABASE hotelmanager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
mysql -u root -p hotelmanager < Database/hotelmanager.sql
powershell -ExecutionPolicy Bypass -File .\run.ps1
```

Sau đó đăng nhập bằng `admin@gmail.com` / `123456`. Nếu mật khẩu MySQL của bạn không phải `123456`, sửa `appsettings.json` trước khi chạy (xem Bước 4).

## Cài đặt từ đầu

### Bước 1. Cài công cụ

| Công cụ | Ghi chú |
|---|---|
| .NET SDK 10 | Tải ở https://dotnet.microsoft.com/download. Phải là **SDK**, không phải Runtime. |
| MySQL Server 8 trở lên | Chạy ở cổng 3306. Cài kèm MySQL Workbench cho dễ thao tác. Nhớ mật khẩu `root` bạn đặt lúc cài. |
| Git | https://git-scm.com |
| Visual Studio Code | Cài extension **C# Dev Kit** (Microsoft). |
| ngrok | Chỉ cần khi thử thanh toán VNPAY (xem Bước 8). |

Kiểm tra sau khi cài, mở terminal **mới**:

```bash
dotnet --version     # phải bắt đầu bằng 10.
mysql --version
git --version
```

Nếu `mysql` báo `not recognized`, thêm thư mục `bin` của MySQL (ví dụ `C:\Program Files\MySQL\MySQL Server 8.0\bin`) vào biến môi trường PATH, hoặc chạy lệnh từ chính thư mục đó.

### Bước 2. Lấy mã nguồn

```bash
git clone https://github.com/Tizkan/nhom1dotnet.git
cd nhom1dotnet
code .
```

Mở đúng thư mục chứa file `nhom1dotnet.sln` và `nhom1dotnet.csproj`, nếu không C# Dev Kit sẽ không nhận project. Nếu VS Code hỏi *"Required assets to build and debug are missing"*, bấm **Yes**.

### Bước 3. Tạo cơ sở dữ liệu

File `Database/hotelmanager.sql` chứa cấu trúc bảng và dữ liệu mẫu nhưng **không có lệnh tạo database**, nên phải tạo database trước rồi mới nạp file.

```bash
mysql -u root -p -e "CREATE DATABASE hotelmanager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
mysql -u root -p hotelmanager < Database/hotelmanager.sql
```

Trên PowerShell, toán tử `<` không dùng được. Dùng lệnh này thay thế:

```powershell
Get-Content Database/hotelmanager.sql -Encoding UTF8 | mysql -u root -p hotelmanager
```

Nếu dùng MySQL Workbench: tạo schema `hotelmanager`, chọn **Server > Data Import > Import from Self-Contained File**, trỏ đến `Database/hotelmanager.sql`, chọn Default Target Schema là `hotelmanager`, bấm **Start Import**.

Kiểm tra lại:

```sql
USE hotelmanager;
SHOW TABLES;
```

Kết quả phải có 9 bảng: `booking_services`, `bookings`, `customers`, `payments`, `room_types`, `rooms`, `services`, `staffs` và `users`.

### Bước 4. Sửa chuỗi kết nối

Mở `appsettings.json` và đổi `user`, `password` cho khớp MySQL trên máy bạn:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=hotelmanager;user=root;password=MAT_KHAU_MYSQL_CUA_BAN"
}
```

Mật khẩu mặc định trong repo là `123456`. Nếu MySQL của bạn đặt mật khẩu khác thì bắt buộc phải sửa, nếu không ứng dụng báo `Access denied` ngay lần truy vấn đầu tiên. Nếu mật khẩu có ký tự `;`, bọc trong dấu nháy đơn: `password='abc;123'`.

Muốn đổi mật khẩu MySQL thành `123456` cho khớp sẵn với file này, đăng nhập MySQL rồi chạy:

```sql
ALTER USER 'root'@'localhost' IDENTIFIED BY '123456';
FLUSH PRIVILEGES;
```

### Bước 5. Khôi phục package và chạy

```bash
dotnet restore
dotnet run --launch-profile https
```

Khi console hiện dòng `Now listening on: https://localhost:7100`, mở trình duyệt vào địa chỉ đó. Profile `http` chạy ở `http://localhost:5153`:

```bash
dotnet run --launch-profile http
```

Muốn tự build lại khi sửa code, dùng `dotnet watch run --launch-profile https`. Trong VS Code, nhấn **F5** cũng chạy được và cho phép đặt breakpoint.

Lần đầu chạy bằng HTTPS, trình duyệt có thể cảnh báo chứng chỉ. Chạy lệnh sau một lần để tin cậy chứng chỉ dev:

```bash
dotnet dev-certs https --trust
```

### Bước 6. Đăng nhập

Tài khoản mẫu có sẵn trong file SQL:

```text
Email:    admin@gmail.com
Mật khẩu: 123456
```

Nhập email vào ô tài khoản. Đăng nhập đúng thì hệ thống chuyển sang trang tổng quan.

### Bước 7. Dừng ứng dụng

Nhấn `Ctrl+C` trong terminal đang chạy.

### Bước 8. Thử thanh toán VNPAY (không bắt buộc)

Sau khi thanh toán xong, VNPAY chuyển **trình duyệt của người thanh toán** về `ReturnUrl`. Cần ngrok vì hai lý do: form đăng ký merchant sandbox của VNPAY không nhận `localhost` ở ô địa chỉ website (báo "Không đúng định dạng Url"), và người thanh toán dùng thiết bị khác (ví dụ điện thoại) không truy cập được localhost của máy chạy ứng dụng.

**Lưu ý quan trọng:** `TmnCode` và `HashSecret` gắn với website đã đăng ký khi tạo tài khoản sandbox. Nếu bạn dùng `TmnCode` trong repo mà địa chỉ website không khớp với địa chỉ đã đăng ký cho mã đó, VNPAY báo **"Website này chưa được phê duyệt"** (`Error.html?code=71`). Hãy dùng đúng cặp `TmnCode`/`HashSecret` của tài khoản mà nhóm đã đăng ký, hoặc đăng ký tài khoản mới theo các bước dưới.

1. **Cài ngrok.** Tìm **ngrok** trong Microsoft Store rồi cài, hoặc tải ở https://download.ngrok.com. Kiểm tra bằng `ngrok version` trong terminal mới.
2. **Kết nối tài khoản ngrok.** Đăng ký ở https://dashboard.ngrok.com, lấy token ở trang **Your Authtoken**, rồi chạy một lần:

   ```bash
   ngrok config add-authtoken TOKEN_CUA_BAN
   ```

3. **Lấy địa chỉ công khai.** Chạy ứng dụng ở profile `http` (cổng 5153), mở terminal khác và chạy:

   ```bash
   ngrok http 5153
   ```

   Copy địa chỉ ở dòng `Forwarding`, dạng `https://....ngrok-free.dev`. Vào mục **Domains** trong dashboard ngrok xem tài khoản có domain cố định không, vì địa chỉ ngẫu nhiên có thể đổi sau mỗi lần chạy lại ngrok.
4. **Đăng ký merchant sandbox** (chỉ khi nhóm chưa có tài khoản) tại https://sandbox.vnpayment.vn/devreg. Ô **Địa chỉ Website** điền địa chỉ ngrok ở bước 3. Mỗi email và mỗi website chỉ đăng ký được một lần: nếu form báo **"Email hoặc website đã được đăng ký"**, tức là đã có tài khoản, hãy tìm email VNPAY gửi lúc đăng ký trước đó thay vì đăng ký lại. Email này chứa `TmnCode` và `HashSecret`.
5. **Sửa mục `Vnpay` trong `appsettings.json`:**

   ```json
   "Vnpay": {
     "TmnCode": "MÃ_TMN_CỦA_BẠN",
     "HashSecret": "HASH_SECRET_CỦA_BẠN",
     "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
     "ReturnUrl": "https://DIA_CHI_NGROK_CUA_BAN/Booking/VnpayReturn"
   }
   ```

   Địa chỉ trong `ReturnUrl` phải **trùng domain** với địa chỉ website đã đăng ký ở bước 4. Sau khi sửa file, **dừng (`Ctrl+C`) và chạy lại** `dotnet run --launch-profile http`, vì ứng dụng chỉ đọc `appsettings.json` lúc khởi động.
6. **Kiểm tra ứng dụng đang gửi đúng thông tin.** Vào Booking, tick một đơn "Chờ xác nhận" và bấm **Thanh toán**. Terminal đang chạy `dotnet run` in dòng bắt đầu bằng `VNPAY URL:`. Trong URL đó, `vnp_TmnCode` phải là mã của bạn và `vnp_ReturnUrl` (đang bị mã hóa, `https%3A%2F%2F...`) phải là địa chỉ ngrok bạn đã đăng ký.
7. **Thanh toán thử.** Trang "Chọn phương thức thanh toán" của VNPAY sandbox mở ra. Chọn **Thẻ nội địa và tài khoản ngân hàng**, chọn ngân hàng NCB, rồi dùng thẻ test trong tài liệu sandbox của VNPAY. Thành công thì trình duyệt quay về trang `VnpayReturn` và đơn đổi sang **Đã xác nhận**.

**Bảo mật:** repo này công khai. Không commit `HashSecret` thật lên GitHub. Cách gọn là dùng user-secrets (chỉ có hiệu lực khi chạy ở môi trường Development, giá trị nằm ngoài repo và ghi đè `appsettings.json` trên máy bạn):

```bash
dotnet user-secrets init
dotnet user-secrets set "Vnpay:TmnCode" "TMN_CODE_CUA_BAN"
dotnet user-secrets set "Vnpay:HashSecret" "HASH_SECRET_CUA_BAN"
```

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

| Lỗi | Nguyên nhân và cách xử lý |
|---|---|
| `Unable to connect to any of the specified MySQL hosts` | MySQL chưa chạy hoặc sai cổng. Mở `services.msc`, bật dịch vụ MySQL (thường tên `MySQL80`), kiểm tra cổng trong chuỗi kết nối. |
| `Unknown database 'hotelmanager'` | Chưa làm Bước 3. Tạo database rồi nạp lại file SQL. |
| `Access denied for user 'root'` | Mật khẩu trong `appsettings.json` khác mật khẩu MySQL thật. Sửa lại ở Bước 4. |
| `Table 'hotelmanager.xxx' doesn't exist` | Đã tạo database nhưng chưa nạp `hotelmanager.sql`. |
| `A compatible .NET SDK was not found` | Máy chưa cài .NET 10. Cài SDK 10 rồi kiểm tra lại bằng `dotnet --version`. |
| `dotnet` không nhận lệnh | Vừa cài xong chưa mở terminal mới. Tắt hẳn VS Code, mở lại. |
| `dotnet restore` không tải được package | Xóa cache NuGet rồi khôi phục lại (lệnh bên dưới). |
| Cảnh báo chứng chỉ HTTPS | Chạy `dotnet dev-certs https --trust`. |
| Thanh toán VNPAY xong nhưng không quay về được ứng dụng | `ReturnUrl` đang trỏ đến địa chỉ ngrok cũ. Làm lại Bước 8. |
| VNPAY báo **Website này chưa được phê duyệt** (`code=71`) | `TmnCode` đang gửi đi không khớp website đã đăng ký, hoặc ứng dụng vẫn đang gửi mã cũ. Xem dòng `VNPAY URL:` trong terminal, đối chiếu `vnp_TmnCode` và `vnp_ReturnUrl`. Sửa `appsettings.json` rồi chạy lại `dotnet run` (Bước 8). |
| Form đăng ký VNPAY báo **Không đúng định dạng Url** | Ô địa chỉ website không nhận `localhost`. Điền địa chỉ ngrok (`https://....ngrok-free.dev`). |
| Form đăng ký VNPAY báo **Email hoặc website đã được đăng ký** | Đã có tài khoản với email hoặc website này. Tìm email VNPAY cũ để lấy `TmnCode` và `HashSecret`, không đăng ký lại. |
| `ngrok` không nhận lệnh | Vừa cài xong chưa mở terminal mới. Đóng hết terminal và VS Code, mở lại rồi chạy `ngrok version`. |

```bash
dotnet nuget locals all --clear
dotnet restore
```

## Nhóm thực hiện

Nhóm 1. Mã nguồn: https://github.com/Tizkan/nhom1dotnet
