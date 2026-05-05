# Requirements Traceability Matrix (RTM) - SaigonRide Project

This document maps the Functional and Non-Functional Requirements to their respective implementation files and test cases.

## 1. Functional Requirements (FRs)

| Req ID | Requirement Description | Owner | Implementation (File/Module) | Test Case ID | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **FR-V01** | Create new Vehicle record (Admin) | Đức | `VehiclesController.cs`, `Views/Vehicles/Create.cshtml` | TC-V01 | Completed |
| **FR-V02** | View & Filter Vehicles list | Đức | `VehiclesController.cs`, `Views/Vehicles/Index.cshtml` | TC-V02 | Completed |
| **FR-V03** | Update Vehicle Status & Station | Đức | `VehiclesController.cs`, `Views/Vehicles/Edit.cshtml` | TC-V03 | Completed |
| **FR-V04** | Delete Vehicle (Only if not In-Transit) | Đức | `VehiclesController.cs` (DeleteConfirmed method) | TC-V04 | Completed |
| **FR-V05** | Auto Transition Status (Available <-> In-Transit) | Đức | `RentalController.cs` (Create/Edit actions) | TC-V05 | Completed |
| **FR-S01** | CRUD Station records | Thành | `StationsController.cs`, `Views/Stations/` | TC-S01 | Completed |
| **FR-S02** | Real-time Current Count & Fill % | Thành | `StationsController.cs`, `Models/Station.cs` | TC-S02 | Completed |
| **FR-S03** | Flag 'Low Inventory' (< 20% capacity) | Thành | `Models/Station.cs` (Property `IsLowInventory`) | TC-S03 | Completed |
| **FR-S04** | Flag 'Almost Full' (> 85% capacity) | Thành | `Models/Station.cs` (Property `IsAlmostFull`) | TC-S04 | Completed |
| **FR-R01** | Fare calculation (500 VND/min Std, 1500 VND/min Scooter) | Đạt | `Services/RentalService.cs` (`CalculateFare`) | TC-R01 | Completed |
| **FR-R02** | 15% Discount for returning to Low Inventory Station | Đạt | `Services/RentalService.cs` (`CheckDiscount`) | TC-R02 | Completed |
| **FR-P01** | Payment via MoMo, VNPay, Cash (Local) | Đạt | `RentalController.cs`, `Views/Rental/Payment.cshtml` | TC-P01 | Completed |
| **FR-P02** | Payment via PayPal, Apple Pay, Cash (Tourist) | Đạt | `RentalController.cs`, `Views/Rental/Payment.cshtml` | TC-P02 | Completed |
| **FR-P03** | Strategy Design Pattern for Payment Gateways | Đạt | `Services/PaymentStrategies/` (Integrated) | TC-P03 | Completed |
| **FR-QA01** | Automated Unit Tests for Pricing Logic (xUnit) | Thành | `SaiGonRide.Tests/RentalServiceTests.cs` | TC-QA01 | Completed |
| **FR-RPT1** | Revenue Report by Category | Team | `AdminDashboardController.cs` | TC-RPT1 | Completed |
| **FR-RPT2** | Station Utilization Report | Team | `AdminDashboardController.cs` | TC-RPT2 | Completed |

## 2. Non-Functional Requirements (NFRs)

| Req ID | Attribute | Measurable Quality Attribute (NFR) | Implementation (File/Module) | Status |
| :--- | :--- | :--- | :--- | :--- |
| **NFR-01** | Performance | Fare calculation & discount return within 1.5s | `Services/RentalService.cs` | Completed |
| **NFR-02** | Security | Password hashing via ASP.NET Core Identity (Bcrypt) | `Program.cs`, `AccountController.cs` | Completed |
| **NFR-03** | Security | Passport numbers encrypted at rest (AES-256) | `ApplicationUser.cs` / Data Security Layer | Completed |
| **NFR-04** | Usability | Fully responsive UI using Bootstrap 5 (375px - 1920px) | `Views/Shared/_Layout.cshtml`, CSS files | Completed |
| **NFR-05** | Maintainability | MVC Architectural Pattern & Strategy Pattern | Project structure | Completed |

---
*Last Updated: 2026-05-05*
