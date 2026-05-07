# 🚲 SaigonRide — Rental Vehicle Service

> **Đồ án môn học:** Kỹ thuật Phần mềm (SE FinalProject 2526)
> **Sinh viên thực hiện:** Trần Hữu Danh — Đại học Tôn Đức Thắng (TDTU)
> **Branch:** `feature/uc03-payment`

SaigonRide (tên kỹ thuật: `RentalVehicleService`) là một nền tảng ứng dụng web hiện đại được xây dựng trên **ASP.NET Core 8.0**, giải quyết nhu cầu di chuyển xanh tại TP. Hồ Chí Minh. Hệ thống cung cấp quy trình thuê xe đạp và xe điện khép kín — từ đăng ký tài khoản, xác thực OTP, quét mã QR thuê xe, theo dõi chuyến đi thời gian thực, đến thanh toán đa phương thức qua VNPay và PayPal.

---

## 🛠️ Công nghệ sử dụng (Tech Stack)

| Thành phần | Công nghệ | Chi tiết |
|---|---|---|
| **Framework** | ASP.NET Core 8.0 | Mô hình MVC kết hợp Razor Pages và Web API |
| **Database** | SQL Server | Quản lý qua Entity Framework Core 8.0, hỗ trợ Migrations và Seeding |
| **ORM** | Entity Framework Core 8.0 | `ApplicationDbContext` với InMemory Database cho Testing |
| **Bảo mật** | ASP.NET Core Identity | Xác thực OTP, phân quyền theo Role (Admin, Tourist, LocalUser) |
| **Thanh toán** | VNPay & PayPal | VNPay via `VNPAY.NET` v2.1.0; PayPal via REST API v2 |
| **Email** | SMTP Gmail | Dịch vụ `SmtpEmailService` gửi OTP và hóa đơn |
| **Kiến trúc** | Strategy & Service Pattern | Quản lý cổng thanh toán và nghiệp vụ linh hoạt |
| **Testing** | xUnit & Moq | Kiểm thử logic tính phí, khuyến mãi và domain model |
| **DevOps** | Docker & Docker Compose | Multi-stage build, hỗ trợ Azure Zip Deploy và MSDeploy |

---

## ✨ Tính năng nổi bật (System Features)

### 1. Hệ thống Hội viên & Xác thực (`AccountController`)

- **Xác thực OTP 2 lớp:** Quy trình đăng ký an toàn với mã xác nhận **6 số** gửi qua Email (SMTP), hiệu lực trong **3 phút**.
- **Phân loại đối tượng người dùng:**
  - `Tourist` — Khách du lịch đăng ký bằng **số hộ chiếu (Passport)** và quốc tịch.
  - `LocalUser` — Người bản địa đăng ký bằng **số định danh cá nhân (CCCD)**.
- **Quản lý Profile:** Cập nhật thông tin cá nhân và theo dõi lịch sử chuyến đi trực quan.

### 2. Quy trình Thuê xe thông minh (`RentalController` & `RentalService`)

- **Quét mã QR:** Xác nhận thông tin xe và trạm trước khi bắt đầu chuyến đi.
- **Theo dõi thời gian thực:** Giám sát chuyến đi đang diễn ra qua trang `ActiveTrip`.
- **Tính phí tự động:** Giá thuê tính theo phút, áp dụng hàm làm tròn lên (`Math.Ceiling`) đảm bảo tính minh bạch.
- **Cơ chế cân bằng trạm (Rebalancing):** Tự động **giảm giá 15%** cho các chuyến đi kết thúc tại trạm đang thiếu xe (`IsLowInventory`) nhằm khuyến khích người dùng tái phân bổ phương tiện.
- **Mã giảm giá (Coupon):** Hỗ trợ áp dụng coupon (ví dụ: `SAIGONGREEN20`) trực tiếp vào tổng hóa đơn.

### 3. Trạng thái Phương tiện (Vehicle Management)

Xe được phân loại và theo dõi theo 4 trạng thái:

| Trạng thái | Mô tả |
|---|---|
| `Available` | Xe sẵn sàng cho thuê |
| `Rented` | Xe đang được thuê |
| `Maintenance` | Xe đang bảo trì |
| `Charging` | Xe đang được sạc pin |

### 4. Hệ thống Trạm xe thông minh (Station System)

- Tự động cập nhật số lượng xe hiện có (`CurrentCount`) so với sức chứa tối đa (`TotalCapacity`).
- **Cảnh báo trạm quá tải:** Khi mật độ xe vượt **90%** công suất.
- **Cảnh báo trạm thiếu xe:** Khi mật độ xe xuống dưới **20%** sức chứa.

### 5. Thanh toán đa phương thức (`PaypalApiController` & `RentalController`)

Hệ thống sử dụng **Payment Strategy Pattern** để chuyển đổi linh hoạt giữa các cổng thanh toán:

- **VNPay:** Tích hợp sâu qua `VNPAY.NET`, xử lý IPN (Instant Payment Notification) và Callback để cập nhật trạng thái chuyến đi. Đơn vị tiền tệ: VND.
- **PayPal:** Tích hợp REST API v2, tạo đơn hàng và capture thanh toán trực tiếp từ server. Hỗ trợ thanh toán quốc tế qua PayPal Business.

### 6. Trung tâm Quản trị Admin (`AdminDashboardController`)

- **Thống kê Dashboard:** Biểu đồ doanh thu **7 ngày** gần nhất, thống kê người dùng mới.
- **Giám sát pin:** Cảnh báo phương tiện có dung lượng pin **dưới 20%**.
- **Quản lý tồn kho trạm:** Theo dõi mật độ xe và cảnh báo bất thường theo thời gian thực.
- **Xuất báo cáo CSV:** Xuất dữ liệu doanh thu và tồn kho trạm ra file CSV với định dạng **UTF-8 BOM**, đảm bảo hiển thị đúng tiếng Việt.

---

## ⚙️ Cấu hình hệ thống (Configuration)

Cập nhật các thông số sau trong tệp `appsettings.json`:

### 1. Kết nối Database

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SaigonRideDB;Trusted_Connection=True;"
}
```

### 2. Cấu hình Email (SMTP Gmail)

```json
"EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "SaigonRide",
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
}
```

> ⚠️ **Lưu ý:** Sử dụng **App Password** của Gmail (không phải mật khẩu tài khoản) để tránh lỗi xác thực.

### 3. Cổng thanh toán VNPay

```json
"VNPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "CallbackUrl": "/Rental/PaymentCallback"
}
```

### 4. Cổng thanh toán PayPal

```json
"PayPal": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "BaseUrl": "https://api-m.sandbox.paypal.com"
}
```

---

## 🚀 Yêu cầu hệ thống & Cài đặt (Setup)

### Yêu cầu

- **.NET SDK 8.0** trở lên
- **Docker Desktop** (khuyên dùng để chạy nhanh)
- **SQL Server** (nếu chạy local không qua Docker)

### Chạy bằng Docker (Khuyến nghị)

```bash
# Khởi chạy toàn bộ hệ thống (App + SQL Server 2022)
docker-compose up --build
```

| Dịch vụ | Cổng |
|---|---|
| Web Application | `http://localhost:5000` (map từ container port `8080`) |
| SQL Server 2022 | `localhost:1433` |

> 🔑 Mật khẩu SQL Server mặc định được cấu hình trong `docker-compose.yml`.

### Chạy local (không Docker)

```bash
# Khôi phục các gói phụ thuộc
dotnet restore

# Áp dụng migrations và tạo database
dotnet ef database update

# Khởi chạy ứng dụng
dotnet run
```

Dữ liệu mẫu sẽ được tự động khởi tạo thông qua `SeedData.Initialize` khi ứng dụng chạy lần đầu tiên.

---

## 📊 Kiểm thử (Testing)

Dự án đi kèm bộ test **`SaiGonRide.Tests`** sử dụng InMemory Database để kiểm tra logic nghiệp vụ mà không ảnh hưởng đến dữ liệu thực:

```bash
dotnet test
```

| Test Suite | Phạm vi kiểm thử |
|---|---|
| `RentalServiceTests` | Độ chính xác tính tiền và áp dụng chiết khấu trạm (Rebalancing) |
| `PromotionTests` | Tính hợp lệ mã giảm giá, đảm bảo hóa đơn không bị âm |
| `DomainModelTests` | Logic cảnh báo pin yếu và tỷ lệ lấp đầy trạm |

---

## 🗄️ Chẩn đoán dữ liệu (Diagnostics)

Dự án đi kèm tệp `DIAGNOSTIC_QUERIES.sql` chứa các truy vấn kiểm tra tính toàn vẹn dữ liệu:

- Kiểm tra đồng bộ giữa trạng thái thuê (`Rental`) và trạng thái xe (`Vehicle`).
- Thống kê doanh thu theo từng phương thức thanh toán.
- Phân tích mật độ xe theo từng trạm.

---

## 🌐 Triển khai (Deployment)

Dự án được cấu hình sẵn các profile để publish lên nhiều môi trường:

| Môi trường | Phương thức |
|---|---|
| **Azure App Service** | Zip Deploy lên Linux x64 |
| **IIS / Windows Server** | Web Deploy (MSDeploy) qua tài khoản `site66889` |
| **Docker** | Multi-stage build tối ưu hóa kích thước image |

---

## 🔄 Vòng đời chuyến đi (Rental Lifecycle)

```
[Quét QR] → [Active] → [Trả xe] → [PendingPayment] → [Thanh toán] → [Completed]
```

---

## 📁 Cấu trúc dự án (Project Structure)

```
RentalVehicleService/
├── Controllers/
│   ├── AccountController.cs        # Đăng ký, đăng nhập, OTP
│   ├── RentalController.cs         # Quy trình thuê xe & thanh toán
│   ├── PaypalApiController.cs      # PayPal REST API integration
│   └── Admin/
│       └── AdminDashboardController.cs  # Quản trị hệ thống
├── Services/
│   ├── RentalService.cs            # Logic tính phí & nghiệp vụ thuê xe
│   ├── SmtpEmailService.cs         # Gửi email OTP & hóa đơn
│   └── Payment/                    # Strategy Pattern cho thanh toán
├── Models/                         # Domain Models & ViewModels
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs                 # Khởi tạo dữ liệu mẫu
├── SaiGonRide.Tests/               # Unit Tests (xUnit & Moq)
├── Dockerfile
├── docker-compose.yml
└── DIAGNOSTIC_QUERIES.sql
```

---

## 👤 Tác giả

**Trương Đỗ Minh Thành,Ngô Huỳnh Đức, Lâm Tấn Đạt **
Sinh viên Đại học Tôn Đức Thắng (TDTU)
Đồ án môn Kỹ thuật Phần mềm — SE FinalProject 2526 | © 2026
