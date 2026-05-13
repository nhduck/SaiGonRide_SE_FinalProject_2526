# SaigonRide – Playwright E2E Automation Test Suite

Comprehensive E2E automation test suite for SaigonRide bike rental application,
covering authentication, admin CRUD operations (stations, vehicles, users, reports),
and a full PayPal rental payment flow.

---

## 📋 Requirements

- **Node.js** v16+
- **@playwright/test** ^1.44.0
- **TypeScript** ^5.4.5
- **SaigonRide app** running on `http://localhost:5072`

---

## 🚀 Installation & Setup

### 1. Install Dependencies

```bash
cd saigonride-playwright-tests
npm install
npx playwright install chromium
```

### 2. Configure BASE_URL

Open `tests/saigonride.spec.ts` and update the constant at the top if needed:

```typescript
const BASE_URL = "http://localhost:5072"; // Change port if different
```

### 3. Start the Application

In a separate terminal, start the SaigonRide web application first:

```bash
cd /path/to/RentalVehicleService
dotnet run
```

Wait until you see `Now listening on: http://localhost:5072` before running tests.

### 4. Run Tests

```bash
# Run all tests (with visible browser window)
npx playwright test --headed

# Run all tests headless (faster, no browser window)
npx playwright test

# Run a specific test suite by name
npx playwright test --grep "Authentication Flows"
npx playwright test --grep "Station Management"
npx playwright test --grep "Vehicle Management"
npx playwright test --grep "User Management"

# Run a single test by ID/name
npx playwright test -g "ST-01: Create New Station"

# View the HTML report after a run
npx playwright show-report
```

---

## 📊 Test Suites

All tests are in a single file: `tests/saigonride.spec.ts`.

### 1. UI & Static Pages ✅

Verify static pages render correctly.

| Test | Route | Assertion |
|------|-------|-----------|
| `Verify Page: How It Works` | `/Home/Guide` | Header contains "How to Rent" |
| `Verify Page: About Us` | `/Home/About` | Header contains "About SaigonRide" |
| `Verify Page: FAQ` | `/Home/FAQ` | Header contains "Frequently Asked" |
| `Verify Page: Privacy Policy` | `/Home/Privacy` | Header contains "Privacy Policy & Terms of Service" |

---

### 2. Authentication Flows 🔐

| ID | Test | Description |
|----|------|-------------|
| TC-AUTH-01 | `TC-AUTH: Registration Process` | Register new user → OTP verification |
| TC-AUTH-02 | `TC-PWD: Reset Password and Login check` | Forgot password → OTP → reset → login |

**Test Accounts:**
Admin:
Email:    admin@saigonride.vn
Password: Admin@123
Customer:
Email:    pokemongo99113@gmail.com
Password: TestPassword123!
PayPal Sandbox (for rental flow):
Email:    thanhtestacc1@gmail.com
Password: thanhtest1

> ⚠️ **OTP Note:** The OTP code in `resetPasswordFlow()` is hardcoded as `"502045"`.
> Replace this with the real OTP received by email if running against a live SMTP setup.

---

### 3. Admin Navigation 👨‍💼

Navigate admin dashboard sections via AJAX sidebar links.

| ID | Test | Endpoint |
|----|------|----------|
| TC-NAV-01 | `Admin -> Stations (AJAX)` | `/Stations/Index` |
| TC-NAV-02 | `Admin -> Vehicles (AJAX)` | `/Vehicles/Index` |
| TC-NAV-03 | `Admin -> Users (AJAX)` | `/UserManagements/Index` |
| TC-NAV-04 | `Admin -> Reports (AJAX)` | `/AdminDashboard/Reports` |

---

### 4. Station Management – CRUD 🏢

| ID | Test | Description |
|----|------|-------------|
| ST-01 | `Create New Station` | Create station with name, address, capacity |
| ST-02 | `Read/View Station Details` | Search → open details |
| ST-03 | `Update Station` | Update capacity: 50 → 75 |
| ST-04 | `Delete Station` | Delete via confirmation modal |

**Test Data:**
```typescript
{
  name: `Test Station ${Date.now()}`,
  address: "District 1, HCMC",
  totalCapacity: "50",
  currentCount: "0"
}
```

---

### 5. Vehicle Management – CRUD 🚍

| ID | Test | Description |
|----|------|-------------|
| VH-01 | `Create New Vehicle` | Create vehicle with model, price, state, battery |
| VH-02 | `Read/View Vehicle Details` | Search → open details modal |
| VH-03 | `Update Vehicle` | Rename to "Honda Air Blade" |
| VH-04 | `Delete Vehicle` | Delete via confirmation modal |

**Test Data:**
```typescript
{
  vehicleModel: "Honda Wave Alpha",
  price: "15000",
  state: "Available",
  batteryPercentage: "80"
}
```

> ⚠️ **Known quirk:** The submit button ID is `#sunmitBtn` (typo in the actual form — do not change).

---

### 6. User Management – CRUD 👥

| ID | Test | Description |
|----|------|-------------|
| US-01 | `Create New User` | Create user with email, username, phone, CCCD |
| US-02 | `Read/View User Details` | Search → open details modal |
| US-03 | `Update User` | Update phone number |
| US-04 | `Delete User` | Delete via confirmation modal |

**Test Data:**
```typescript
{
  email: `testuser${Date.now()}@saigonride.vn`,
  username: `testuser${Date.now()}`,
  fullname: "Test User",
  phone: "0987654321",
  cccd: "123456789012",
  password: "TestPassword123!"
}
```

> ⚠️ **Search bar** (`#userSearchBar`) searches by **username**, not email.

---

### 7. Report Management – CRUD 📋

**RP-01: Create Report (Customer UI)**
- Route: `/Home/ReportIssue`
- Fill: Title, Description → Submit → expect success toast

**RP-02 to RP-05: Admin Operations**

| ID | Test | Description |
|----|------|-------------|
| RP-02 | `View Report Details` | Search → view details |
| RP-03 | `Export/Download Reports` | Click export → file downloaded |
| RP-04 | `Filter Reports by Date Range` | Filter by StartDate / EndDate |
| RP-05 | `Delete Report` | Delete via confirm dialog |

---

### 8. Bike Rental + PayPal Payment 💳

Full end-to-end rental flow:
Customer login
→ Search station → select station
→ Select vehicle
→ Mock QR scan: /Rental/Create?vehicleId=4&startStationId=30
→ Start ride → confirm
→ Select end station → Finish
→ Choose PayPal payment
→ PayPal sandbox login → approve
→ Verify payment success page

---

## 🔧 Helper Functions

### `loginAs(page, email, password)`
Navigates to the login page and signs in with any credentials.

### `loginAsAdmin(page)`
Logs in as admin and navigates to `/AdminDashboard/Index`.

### `searchAndGetRow(page, selectorSearch, searchText, rowText?)`
Fires an `input` event on the search bar to trigger AJAX immediately (bypasses browser debounce), then waits for the result `<tr>` to appear. **Always use this instead of `.fill()` + manual wait** for AJAX-driven tables.

```typescript
const row = await searchAndGetRow(page, '#stationSearchBar', 'Test Station 123');
```

### `registerAndVerify(page, email, password)`
Registers a new user and completes OTP verification.

### `resetPasswordFlow(page, email, newPassword)`
Runs the full forgot-password flow: submit email → enter OTP → reset → redirects to login.

### `expectSuccess(page, timeout?)`
Waits for a success toast notification (checks multiple CSS selectors).

---

## ⚙️ Playwright Configuration (`playwright.config.ts`)

```typescript
export default defineConfig({
  testDir: './tests',
  fullyParallel: false,   // Tests run sequentially — required to avoid DB conflicts
  workers: 1,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5072',
    video: 'on',
    screenshot: 'only-on-failure',
    trace: 'on-first-retry',
    ignoreHTTPSErrors: true,
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
```

> **Why `workers: 1`?** All tests share a single PostgreSQL database. Running in parallel causes race conditions and false failures on CRUD operations.

---

## ⚠️ Important Notes

| Issue | Detail |
|-------|--------|
| **App must be running** | Run `dotnet run` before executing any tests |
| **Single spec file** | All tests are in `tests/saigonride.spec.ts` |
| **Workers = 1** | Sequential execution is mandatory — parallel runs cause DB conflicts |
| **AJAX search** | Always use `searchAndGetRow()` — direct `.fill()` won't trigger the AJAX handler |
| **No toast on some forms** | Station/vehicle edit forms don't show a success toast; wait for modal close or page reload instead |
| **OTP hardcoded** | `"502045"` in `resetPasswordFlow()` — replace with real OTP if SMTP is live |
| **Submit button typo** | Vehicle/station forms use `#sunmitBtn` (typo in source HTML — correct in tests) |
| **i18n attributes** | `data-i18n` values are processed by JS; use `.toContainText()` not exact text match |

---

## 📁 Project Structure
saigonride-playwright-tests/
├── tests/
│   └── saigonride.spec.ts     ← All test cases (single file)
├── playwright-report/          ← HTML report (generated after run)
├── test-results/               ← Artifacts: screenshots, videos, traces
├── playwright.config.ts        ← Playwright configuration
├── package.json                ← Dependencies & scripts
└── README.md                   ← This file

---

## 🎯 Quick Start

```bash
# Terminal 1 — start the app
cd RentalVehicleService
dotnet run

# Terminal 2 — run tests
cd saigonride-playwright-tests
npm install
npx playwright install chromium
npx playwright test --headed

# View results
npx playwright show-report
```