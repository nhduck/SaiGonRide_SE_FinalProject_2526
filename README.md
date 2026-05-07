# 🚲 SaigonRide — Rental Vehicle Service

<<<<<<< HEAD
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
=======
> **Course Project:** Software Engineering (SE FinalProject 2526)
> **Author:** Le Minh Thanh, Ngo Huynh Duc, Lam Tan Dat — Ton Duc Thang University (TDTU)

SaigonRide (technical name: `RentalVehicleService`) is a modern web application built on **ASP.NET Core 8.0**, designed to address the urban mobility needs of Ho Chi Minh City. The platform provides an end-to-end bicycle and electric vehicle rental experience — from account registration with OTP verification, QR code scanning, real-time trip tracking, to multi-gateway payment processing via VNPay and PayPal.

---

## 🛠️ Tech Stack

| Component | Technology | Details |
|---|---|---|
| **Framework** | ASP.NET Core 8.0 | MVC pattern with Razor Pages and Web API |
| **Database** | SQL Server | Managed via Entity Framework Core 8.0 with Migrations & Seeding |
| **ORM** | Entity Framework Core 8.0 | `ApplicationDbContext`; InMemory Database for testing |
| **Security** | ASP.NET Core Identity | OTP authentication, Role-based authorization (Admin, Tourist, LocalUser) |
| **Payments** | VNPay & PayPal | VNPay via `VNPAY.NET` v2.1.0; PayPal via REST API v2 |
| **Email** | SMTP Gmail | `SmtpEmailService` for OTP delivery and invoice notifications |
| **Architecture** | Strategy & Service Pattern | Flexible payment gateway and business logic management |
| **Testing** | xUnit & Moq | Unit tests for billing logic, promotions, and domain models |
| **DevOps** | Docker & Docker Compose | Multi-stage build; supports Azure Zip Deploy and MSDeploy |

---

## ✨ System Features

### 1. Membership & Authentication (`AccountController`)

- **Two-factor OTP Verification:** Secure registration flow using a **6-digit confirmation code** sent via Email (SMTP), valid for **3 minutes**.
- **User Type Classification:**
  - `Tourist` — Foreign visitors register using a **Passport number** and nationality.
  - `LocalUser` — Local residents register using a **National ID (CCCD)**.
- **Profile Management:** Update personal information and view trip history with a clean, visual interface.

### 2. Smart Rental Workflow (`RentalController` & `RentalService`)

- **QR Code Scanning:** Validates vehicle and station information before a trip begins.
- **Real-time Trip Tracking:** Monitors the active journey via the `ActiveTrip` view.
- **Automatic Billing:** Rental fees are calculated per minute using `Math.Ceiling` rounding for full transparency.
- **Station Rebalancing Incentive:** Automatically applies a **15% discount** on trips that end at low-inventory stations (`IsLowInventory`), encouraging users to return vehicles where they are needed most.
- **Coupon Support:** Users can apply discount codes (e.g., `SAIGONGREEN20`) directly to the total invoice at checkout.

### 3. Vehicle Management

Vehicles are tracked across four distinct states:

| Status | Description |
|---|---|
| `Available` | Ready to be rented |
| `Rented` | Currently in use |
| `Maintenance` | Undergoing repair or servicing |
| `Charging` | Currently charging the battery |

### 4. Smart Station System

- Continuously syncs the current vehicle count (`CurrentCount`) against each station's total capacity (`TotalCapacity`).
- **Overcrowding Alert:** Triggered when station occupancy exceeds **90%** of capacity.
- **Low Inventory Alert:** Triggered when station occupancy drops below **20%** of capacity.

### 5. Multi-gateway Payment Processing (`PaypalApiController` & `RentalController`)

The system uses the **Payment Strategy Pattern** to switch seamlessly between payment providers:

- **VNPay:** Deep integration via the `VNPAY.NET` library, handling IPN (Instant Payment Notification) and Callback to update trip status. Currency: VND.
- **PayPal:** Full REST API v2 integration for order creation and server-side payment capture. Supports international payments via PayPal Business.

### 6. Admin Control Center (`AdminDashboardController`)

- **Dashboard Analytics:** Revenue chart for the **last 7 days**, new user statistics.
- **Battery Monitoring:** Alerts for any vehicle with a battery level **below 20%**.
- **Station Inventory Management:** Real-time tracking of vehicle density with anomaly alerts.
- **CSV Report Export:** Export revenue and station inventory data to CSV files encoded in **UTF-8 BOM** for proper Vietnamese character rendering.

---

## ⚙️ Configuration

Update the following settings in `appsettings.json` to enable all features:

### 1. Database Connection
>>>>>>> bb00869c9505c873f2f05dffdd1a843218bfe2ca

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SaigonRideDB;Trusted_Connection=True;"
}
```

<<<<<<< HEAD
### 2. Cấu hình Email (SMTP Gmail)
=======
### 2. Email Settings (SMTP Gmail)
>>>>>>> bb00869c9505c873f2f05dffdd1a843218bfe2ca

```json
"EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "SaigonRide",
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
}
```

<<<<<<< HEAD
> ⚠️ **Lưu ý:** Sử dụng **App Password** của Gmail (không phải mật khẩu tài khoản) để tránh lỗi xác thực.

### 3. Cổng thanh toán VNPay
=======
> ⚠️ **Note:** Use a Gmail **App Password** (not your account password) to avoid authentication errors.

### 3. VNPay Gateway
>>>>>>> bb00869c9505c873f2f05dffdd1a843218bfe2ca

```json
"VNPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "CallbackUrl": "/Rental/PaymentCallback"
}
```

<<<<<<< HEAD
### 4. Cổng thanh toán PayPal
=======
### 4. PayPal Gateway
>>>>>>> bb00869c9505c873f2f05dffdd1a843218bfe2ca

```json
"PayPal": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "BaseUrl": "https://api-m.sandbox.paypal.com"
}
```

---

<<<<<<< HEAD
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
=======
## 🚀 Getting Started

### Prerequisites

- **.NET SDK 8.0** or later
- **Docker Desktop** (recommended for quick setup)
- **SQL Server** (required if running locally without Docker)

### Run with Docker (Recommended)

```bash
# Start the full stack (App + SQL Server 2022)
docker-compose up --build
```

| Service | Port |
|---|---|
| Web Application | `http://localhost:5000` (mapped from container port `8080`) |
| SQL Server 2022 | `localhost:1433` |

> 🔑 The default SQL Server password is defined in `docker-compose.yml`.

### Run Locally (Without Docker)

```bash
# Restore dependencies
dotnet restore

# Apply migrations and create the database
dotnet ef database update

# Start the application
dotnet run
```

Sample data will be automatically seeded via `SeedData.Initialize` on the first run.

---

## 📊 Testing

The project includes a dedicated **`SaiGonRide.Tests`** suite using an InMemory Database to validate business logic without affecting production data:
>>>>>>> bb00869c9505c873f2f05dffdd1a843218bfe2ca

```bash
dotnet test
```

<<<<<<< HEAD
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
=======
| Test Suite | Coverage |
|---|---|
| `RentalServiceTests` | Billing accuracy and station rebalancing discount logic |
| `PromotionTests` | Coupon validity and prevention of negative invoice totals |
| `DomainModelTests` | Low battery alerts and station occupancy rate calculations |

---

## 🗄️ Data Diagnostics

The project ships with a `DIAGNOSTIC_QUERIES.sql` file containing queries to verify data integrity:

- Synchronization check between rental status and vehicle status.
- Revenue breakdown by payment method.
- Vehicle density analysis per station.

---

## 🌐 Deployment

The project includes pre-configured publish profiles for multiple environments:

| Environment | Method |
|---|---|
| **Azure App Service** | Zip Deploy to Linux x64 |
| **IIS / Windows Server** | Web Deploy (MSDeploy) via account `site66889` |
| **Docker** | Optimized multi-stage build |

---

## 🔄 Rental Lifecycle

```
[Scan QR] → [Active] → [Return Vehicle] → [PendingPayment] → [Pay] → [Completed]
>>>>>>> bb00869c9505c873f2f05dffdd1a843218bfe2ca
```

---

<<<<<<< HEAD
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
=======
## 📁 Project Structure

```
RentalVehicleService/
│
├── 📁 Controllers/                          # HTTP Request Handlers
│   ├── AccountController.cs                 # Registration, login, OTP verification
│   ├── RentalController.cs                  # Rental workflow & payment processing
│   ├── PaypalApiController.cs               # PayPal REST API integration
│   └── 📁 Admin/
│       └── AdminDashboardController.cs      # Admin dashboard & analytics
│
├── 📁 Services/                             # Business Logic & Domain Services
│   ├── RentalService.cs                     # Billing logic, rebalancing, coupon
│   ├── SmtpEmailService.cs                  # OTP & invoice email delivery
│   ├── AccountService.cs                    # Authentication & account management
│   ├── VehicleService.cs                    # Vehicle state management
│   ├── StationService.cs                    # Station management
│   └── 📁 Payment/                          # Strategy Pattern for payment gateways
│       ├── IPaymentStrategy.cs              # Payment strategy interface
│       ├── VNPayPaymentStrategy.cs          # VNPay implementation
│       ├── PayPalPaymentStrategy.cs         # PayPal implementation
│       └── WalletPaymentStrategy.cs         # E-wallet implementation
│
├── 📁 Models/                               # Domain Models & ViewModels
│   ├── 📁 Domain/                           # Core business entities
│   │   ├── ApplicationUser.cs               # User model (inherits IdentityUser)
│   │   ├── Vehicle.cs                       # Vehicle entity
│   │   ├── Station.cs                       # Station entity
│   │   ├── Rental.cs                        # Trip/rental entity
│   │   ├── Payment.cs                       # Payment entity
│   │   ├── Coupon.cs                        # Discount coupon entity
│   │   └── Rating.cs                        # User rating entity
│   └── 📁 ViewModels/                       # DTOs for Razor Pages & API
│       ├── RentalViewModel.cs
│       ├── PaymentViewModel.cs
│       ├── AdminDashboardViewModel.cs
│       └── UserProfileViewModel.cs
│
├── 📁 Data/                                 # Database & EF Core
│   ├── ApplicationDbContext.cs              # Main DbContext
│   ├── SeedData.cs                          # Sample data initializer (Vehicles, Stations)
│   └── 📁 Migrations/                       # Database migration history
│       ├── [Timestamp]_InitialCreate.cs
│       ├── [Timestamp]_AddPaymentFeatures.cs
│       └── ...
│
├── 📁 Pages/                                # Razor Pages (Presentation Layer)
│   ├── Index.cshtml                         # Home page
│   ├── Index.cshtml.cs
│   ├── 📁 Account/                          # Authentication & account pages
│   │   ├── Login.cshtml / .cs
│   │   ├── Register.cshtml / .cs
│   │   ├── VerifyOtp.cshtml / .cs
│   │   └── Profile.cshtml
│   ├── 📁 Rental/                           # Trip management pages
│   │   ├── Index.cshtml                     # Trip list
│   │   ├── ScanQR.cshtml                    # QR code scanner
│   │   ├── ActiveTrip.cshtml                # Real-time active trip view
│   │   ├── Payment.cshtml                   # Payment page (VNPay & PayPal)
│   │   ├── PaymentSuccess.cshtml
│   │   ├── PaymentFailed.cshtml
│   │   └── RentalHistory.cshtml
│   ├── 📁 Admin/                            # Admin management pages
│   │   ├── Dashboard.cshtml                 # Revenue & user statistics
│   │   ├── Vehicles.cshtml                  # Vehicle management
│   │   ├── Stations.cshtml                  # Station management
│   │   ├── Users.cshtml                     # User management
│   │   └── Reports.cshtml                   # CSV report export
│   ├── 📁 Shared/                           # Layouts & shared components
│   │   ├── _Layout.cshtml                   # Main layout
│   │   ├── _LoginPartial.cshtml
│   │   ├── _Navigation.cshtml
│   │   ├── _Footer.cshtml
│   │   └── 📁 Components/                   # Reusable UI components
│   │       ├── AlertComponent.cshtml
│   │       ├── LoadingSpinner.cshtml
│   │       └── PaginationComponent.cshtml
│   └── 📁 Error/
│       └── Error.cshtml
│
├── 📁 wwwroot/                              # Static Files (CSS, JS, Images)
│   ├── 📁 css/
│   │   ├── site.css                         # Main stylesheet
│   │   ├── payment.css                      # Payment page styles
│   │   ├── responsive.css                   # Responsive design
│   │   └── bootstrap.min.css
│   ├── 📁 js/
│   │   ├── site.js                          # Global JavaScript
│   │   ├── payment.js                       # Payment logic (VNPay & PayPal)
│   │   ├── map.js                           # GPS tracking & mapping
│   │   ├── qr-scanner.js                    # QR code scanner
│   │   └── real-time-tracker.js             # Real-time trip tracking
│   ├── 📁 images/
│   │   ├── logo.png
│   │   ├── vnpay-logo.png
│   │   ├── paypal-logo.png
│   │   ├── background.jpg
│   │   └── 📁 icons/
│   │       ├── bike-icon.svg
│   │       ├── station-icon.svg
│   │       └── 📁 payment-icons/
│   └── 📁 lib/                              # Third-party frontend libraries
│       ├── bootstrap/
│       ├── jquery/
│       ├── popper.js/
│       └── qrcode.js/
│
├── 📁 SaiGonRide.Tests/                     # Unit & Integration Tests
│   ├── 📁 Services/
│   │   ├── RentalServiceTests.cs            # Billing & rebalancing logic tests
│   │   ├── PaymentServiceTests.cs           # Payment processing tests
│   │   └── EmailServiceTests.cs             # Email delivery tests
│   ├── 📁 Models/
│   │   ├── VehicleTests.cs                  # Vehicle business logic tests
│   │   ├── StationTests.cs                  # Station business logic tests
│   │   └── RentalTests.cs                   # Rental calculation tests
│   ├── 📁 Controllers/
│   │   ├── AccountControllerTests.cs
│   │   ├── RentalControllerTests.cs
│   │   └── PaymentControllerTests.cs
│   ├── 📁 Fixtures/                         # Test data & mocks
│   │   ├── MockUserData.cs
│   │   ├── MockVehicleData.cs
│   │   ├── TestDbContext.cs                 # InMemory DB for testing
│   │   └── FakeEmailSender.cs
│   └── Usings.cs                            # Global usings for tests
│
├── 📄 Program.cs                            # Application entry point & startup config
├── 📄 appsettings.json                      # Main configuration (DB, payment keys)
├── 📄 appsettings.Development.json          # Development-specific config
├── 📄 appsettings.Production.json           # Production config
├── 📄 Dockerfile                            # Docker image configuration
├── 📄 docker-compose.yml                    # Multi-container orchestration (App + SQL Server)
├── 📄 DIAGNOSTIC_QUERIES.sql               # SQL queries for data integrity checks
├── 📄 .gitignore
└── 📄 RentalVehicleService.csproj          # Project configuration & NuGet dependencies
```

---
## 👤 Author

**Le Minh Thanh, Ngo Huynh Duc, Lam Tan Dat**
Ton Duc Thang University (TDTU)
Software Engineering Course Project — SE FinalProject 2526 | © 2026

>>>>>>> bb00869c9505c873f2f05dffdd1a843218bfe2ca
