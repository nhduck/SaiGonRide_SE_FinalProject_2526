# SaigonRide – Playwright E2E Automation Test Suite

Comprehensive automation test suite for SaigonRide bike rental application, covering authentication, resource management (stations, vehicles, users), and payment flows with PayPal integration.

---

## 📋 Requirements

- **Node.js** v16+
- **Playwright** @latest
- **Application** running on `http://localhost:5072` (configurable)

---

## 🚀 Installation & Setup

### 1. Install Dependencies

```bash
npm install -D @playwright/test
npx playwright install chromium
```

### 2. Configure BASE_URL

Open `tests/saigonride.specs.ts` and update the first line if needed:

```typescript
const BASE_URL = "http://localhost:5072"; // Change port if different
```

### 3. Run Tests

```bash
# Run all tests (display browser)
npx playwright test --headed

# Run tests headless (faster, no browser window)
npx playwright test

# Run specific test suite
npx playwright test --grep "Authentication Flows"
npx playwright test --grep "Station Management"
npx playwright test --grep "Vehicle Management"
npx playwright test --grep "User Management"

# Run single test
npx playwright test -g "ST-01: Create New Station"

# View HTML report
npx playwright show-report
```

---

## 📊 Test Suites

### 1. **UI & Static Pages** ✅

Verify static pages display correctly:
- How It Works (`/Home/Guide`)
- About Us (`/Home/About`)
- FAQ (`/Home/FAQ`)
- Privacy Policy (`/Home/Privacy`)

| Test | Description |
|------|-------------|
| `Verify Page: How It Works` | Guide page header and content |
| `Verify Page: About Us` | About Us page header and content |
| `Verify Page: FAQ` | FAQ page header and content |
| `Verify Page: Privacy Policy` | Privacy Policy page header |

---

### 2. **Authentication Flows** 🔐

| ID | Test | Description |
|----|------|-------------|
| TC-AUTH-01 | `TC-AUTH: Registration Process` | Register new user + OTP verification |
| TC-AUTH-02 | `TC-PWD: Reset Password and Login check` | Forgot password → reset → login |

**Test Accounts:**
```
Admin:
  Email: admin@saigonride.vn
  Password: Admin@123

Customer:
  Email: pokemongo99113@gmail.com
  Password: TestPassword123!

PayPal (for rental test):
  Email: thanhtestacc1@gmail.com
  Password: thanhtest1
```

---

### 3. **Admin Navigation** 👨‍💼

Navigate admin dashboard menu items via AJAX:

| ID | Test | Endpoint |
|----|------|----------|
| TC-NAV-01 | `Admin -> Stations (AJAX)` | `/Stations/Index` |
| TC-NAV-02 | `Admin -> Vehicles (AJAX)` | `/Vehicles/Index` |
| TC-NAV-03 | `Admin -> Users (AJAX)` | `/UserManagements/Index` |
| TC-NAV-04 | `Admin -> Reports (AJAX)` | `/AdminDashboard/Reports` |

---

### 4. **Station Management – CRUD** 🏢

Complete CRUD operations for bike stations.

| ID | Test | Description |
|----|------|-------------|
| ST-01 | `Create New Station` | Create station: name, address, capacity |
| ST-02 | `Read/View Station Details` | Search → view station details |
| ST-03 | `Update Station` | Update capacity from 50 → 75 |
| ST-04 | `Delete Station` | Delete station via confirmation modal |

**Test Data:**
```typescript
{
  name: `Test Station ${Date.now()}`,
  address: "District 1, HCMC",
  totalCapacity: "50",
  currentCount: "0"
}
```

**Key Notes:**
- Use `searchAndGetRow()` to trigger AJAX search with proper waits
- Search returns `<tr>` element containing station name

---

### 5. **Vehicle Management – CRUD** 🚍

Complete CRUD operations for vehicles.

| ID | Test | Description |
|----|------|-------------|
| VH-01 | `Create New Vehicle` | Create vehicle: model, price, state, battery |
| VH-02 | `Read/View Vehicle Details` | Search → view details modal |
| VH-03 | `Update Vehicle` | Rename vehicle to "Honda Air Blade" |
| VH-04 | `Delete Vehicle` | Delete vehicle via confirmation modal |

**Test Data:**
```typescript
{
  vehicleModel: "Honda Wave Alpha",
  price: "15000",
  state: "Available",
  batteryPercentage: "80"
}
```

**Key Notes:**
- Modals: `#createModal`, `#editModal`
- Submit button: `#sunmitBtn` (typo in form)
- Some forms don't show toast on success → wait for modal close or page reload

---

### 6. **User Management – CRUD** 👥

Complete CRUD operations for user accounts.

| ID | Test | Description |
|----|------|-------------|
| US-01 | `Create New User` | Create user: email, username, phone, CCCD |
| US-02 | `Read/View User Details` | Search → view details modal |
| US-03 | `Update User` | Update phone number |
| US-04 | `Delete User` | Delete user via confirmation modal |

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

**Key Notes:**
- Search bar `#userSearchBar` searches by username, **not email**
- Modals: `#createUserModal`, `#editUserModal`, `#deleteUserModal`

---

### 7. **Report Management – CRUD** 📋

#### **RP-01: Create New Report (User UI)**

```
Page: /Home/ReportIssue
Test Data:
  title: `Test Report ${Date.now()}`
  description: "This is a test report"
  status: "Open"
```

Steps:
1. Customer login
2. Navigate to `/Home/ReportIssue`
3. Fill Title, Description, CreatedDate (auto: today)
4. Click Submit → success toast appears

#### **RP-02 to RP-05: Admin Operations**

| ID | Test | Description |
|----|------|-------------|
| RP-02 | `View Report Details` | Search report → view details |
| RP-03 | `Export/Download Reports` | Click export button |
| RP-04 | `Filter Reports by Date Range` | Filter by StartDate/EndDate |
| RP-05 | `Delete Report` | Delete report via confirm dialog |

---

### 8. **Bike Rental + PayPal Payment** 💳

End-to-end rental flow with PayPal payment integration.

| Test | Steps |
|------|-------|
| `TC-RENT: Bike Rental with PayPal Payment` | 1. Customer login |
| | 2. Station search → select station |
| | 3. View list → select vehicle |
| | 4. Scan QR (mock: redirect to `/Rental/Create?vehicleId=4&startStationId=30`) |
| | 5. Start ride → confirm |
| | 6. Select end station → Finish |
| | 7. Pay with PayPal |
| | 8. PayPal login → approve payment |
| | 9. Verify success page |

**PayPal Test Credentials:**
```
Email: thanhtestacc1@gmail.com
Password: thanhtest1
```

---

## 🔧 Helper Functions

### `loginAs(page, email, password)`
Login with any email/password combination.

```typescript
await loginAs(page, "admin@saigonride.vn", "Admin@123");
```

### `loginAsAdmin(page)`
Login as admin and navigate to Admin Panel.

```typescript
await loginAsAdmin(page);
// Auto redirects to /AdminDashboard/Index
```

### `searchAndGetRow(page, selectorSearch, searchText, rowText?)`
**Fixes AJAX timeout issues:**
- Fill search box
- Trigger `input` event → AJAX fires immediately (no browser debounce wait)
- Wait for row to appear (reliable signal that AJAX completed)

```typescript
const row = await searchAndGetRow(
  page, 
  '#stationSearchBar', 
  'Test Station 123'
);
```

### `registerAndVerify(page, email, password)`
Register new user and verify via OTP.

```typescript
await registerAndVerify(page, "newuser@test.com", "Pass123!");
```

### `resetPasswordFlow(page, email, newPassword)`
Complete forgot password flow: forgot → OTP verify → reset → login page.

```typescript
await resetPasswordFlow(page, "user@test.com", "NewPass123!");
```

### `expectSuccess(page, timeout?)`
Wait for success toast (checks multiple selectors).

```typescript
await expectSuccess(page, 10000);
```

---

## ⚙️ Playwright Configuration

Edit `playwright.config.ts`:

```typescript
export default defineConfig({
  testDir: './tests', // Tests in tests/ directory
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : 1, // ✅ Run sequentially to avoid DB conflicts
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5072',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    ignoreHTTPSErrors: true, // ✅ For localhost SSL
  },
  webServer: {
    command: 'dotnet run', // Optional: auto-start app
    url: 'http://localhost:5072',
    reuseExistingServer: !process.env.CI,
  },
});
```

---

## ⚠️ Important Notes

| Point | Detail |
|-------|--------|
| **App must run first** | `dotnet run` before starting tests |
| **BASE_URL config** | Update if port differs from 5072 |
| **Workers: 1** | Run sequentially to avoid shared DB conflicts |
| **AJAX Search** | Use `searchAndGetRow()` (dispatches event + waits) |
| **No Toast on some forms** | Station/vehicle edit → wait for page reload or modal close |
| **OTP Code** | Hardcoded `"502045"` → replace with real OTP if different |
| **i18n Processing** | `data-i18n` attributes processed by JS → use `toContainText()` for assertions |
| **Entity Modals** | Each entity has separate modals (`#createModal`, `#editModal`, etc) |
| **Headless Mode** | Add `--headed=false` or config `headless: true` for fast CI runs |

---

## 📁 Project Structure

```
saigonride-playwright-tests/
├── tests/
│   └── saigonride.specs.ts       ← Main test file
├── playwright-report/             ← HTML test reports (generated)
├── test-results/                  ← Test results (generated)
├── playwright.config.ts           ← Playwright configuration
├── package.json                   ← Dependencies
└── README.md                      ← This file
```

---

## 🎯 Quick Start

```bash
# 1. Start the app
cd /path/to/saigonride
dotnet run

# 2. Run tests (in another terminal)
cd saigonride-playwright-tests
npx playwright test --headed

# 3. View results
npx playwright show-report
```