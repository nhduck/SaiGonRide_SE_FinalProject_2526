# 🚲 SaigonRide — Rental Vehicle Service

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

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SaigonRideDB;Trusted_Connection=True;"
}
```

### 2. Email Settings (SMTP Gmail)

```json
"EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "SaigonRide",
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
}
```

> ⚠️ **Note:** Use a Gmail **App Password** (not your account password) to avoid authentication errors.

### 3. VNPay Gateway

```json
"VNPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "CallbackUrl": "/Rental/PaymentCallback"
}
```

### 4. PayPal Gateway

```json
"PayPal": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "BaseUrl": "https://api-m.sandbox.paypal.com"
}
```

---

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

```bash
dotnet test
```

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
```

---

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
