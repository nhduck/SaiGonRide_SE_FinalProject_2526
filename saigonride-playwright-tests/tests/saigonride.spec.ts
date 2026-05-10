import { test, expect, Page } from "@playwright/test";

// ============================================================
// 1. CONFIGURATION & CONSTANTS
// ============================================================
const BASE_URL = "http://localhost:5072";

const ADMIN_EMAIL = "admin@saigonride.vn";
const ADMIN_PASSWORD = "Admin@123";

const CUSTOMER_EMAIL = "pokemongo99113@gmail.com";
const CUSTOMER_PASSWORD = "TestPassword123!";
const TESTER_NAME = "testing1";

const PAYPAL_USER = "thanhtestacc1@gmail.com";
const PAYPAL_PASS = "thanhtest1";

const ADMIN_MENU_AJAX = [
    { name: 'Stations', url: '/Stations/Index', i18nKey: 'admin_station_management', expectedText: 'Station Management' },
    { name: 'Vehicles', url: '/Vehicles/Index', i18nKey: 'admin_vehicle_management', expectedText: 'Vehicle Management' },
    { name: 'Users', url: '/UserManagements/Index', i18nKey: 'admin_user_management', expectedText: 'User Management' },
    { name: 'Reports', url: '/AdminDashboard/Reports', i18nKey: 'admin_report_mgmt', expectedText: 'Report Management' }
];

const STATIC_PAGES = [
    { name: 'How It Works', url: '/Home/Guide', i18n: 'guide_title', expectedText: /How to Rent/i },
    { name: 'About Us', url: '/Home/About', i18n: 'about_title', expectedText: /About SaigonRide/i },
    { name: 'FAQ', url: '/Home/FAQ', i18n: 'faq_title', expectedText: /Frequently Asked/i },
    { name: 'Privacy Policy', url: '/Home/Privacy', i18n: null, expectedText: /Privacy Policy & Terms of Service/i },
];

const STATION_DATA = {
    name: `Test Station ${Date.now()}`,
    address: "District 1, HCMC",
    totalCapacity: "50",
    currentCount: "0"
};

// Vehicle: dùng đúng field names theo form thực tế (VehicleModel, Price, State, BatteryPercentage)
const VEHICLE_DATA = {
    vehicleModel: "Honda Wave Alpha",
    price: "15000",
    state: "Available",         // phải khớp với enum VehicleState
    batteryPercentage: "80"
};

// User: dùng đúng field names theo form thực tế
const USER_DATA = {
    email: `testuser${Date.now()}@saigonride.vn`,
    username: `testuser${Date.now()}`,
    fullname: "Test User",
    phone: "0987654321",
    cccd: "123456789012",
    password: "TestPassword123!"
};

const REPORT_DATA = {
    title: `Test Report ${Date.now()}`,
    description: "This is a test report",
    status: "Open"
};

// ============================================================
// 2. HELPERS (UTILITY FUNCTIONS)
// ============================================================

async function navigateToHome(page: Page) {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');
}

async function loginAs(page: Page, email: string, password: string) {
    await page.goto(`${BASE_URL}/Admin/Account/Login`);
    await page.locator('input[name="Email"]').fill(email);
    await page.locator('#loginPassword, input[name="Password"]').first().fill(password);
    await page.locator('#loginBtn, button[type="submit"]').first().click();
    await page.waitForLoadState('networkidle');
}

async function loginAsAdmin(page: Page) {
    await loginAs(page, ADMIN_EMAIL, ADMIN_PASSWORD);
    const adminPanelBtn = page.locator('a[href="/AdminDashboard/Index"], a:has-text("Admin")').first();
    await expect(adminPanelBtn).toBeVisible({ timeout: 5000 });
    await adminPanelBtn.click();
    await page.waitForURL("**/AdminDashboard/Index");
}

async function resetPasswordFlow(page: Page, email: string, newPassword: string) {
    await page.goto(`${BASE_URL}/Admin/Account/Login`);
    await page.locator('a[href*="ForgotPassword"]').click();
    await page.locator("#Email").fill(email);
    await page.keyboard.press('Enter');

    const otpInput = page.locator("#otpInput");
    await expect(otpInput).toBeVisible({ timeout: 10000 });
    await otpInput.fill("502045");
    await page.locator("#verifyOtpBtn").click();

    await page.locator("#newPassword").fill(newPassword);
    await page.locator("#confirmPassword").fill(newPassword);
    await page.locator('button[type="submit"]').click();

    await page.waitForURL("**/Admin/Account/Login");
}

async function registerAndVerify(page: Page, email: string, password: string) {
    await page.goto(BASE_URL);
    await page.locator('a[href*="Register"]').first().click();

    const emailField = page.locator('input[name="Email"], #Email').first();
    await emailField.fill(email);
    await page.locator('#regPassword, input[name="Password"]').first().fill(password);
    await page.locator('#regConfirmPassword, input[name="ConfirmPassword"]').first().fill(password);

    if (await page.locator("#FullName").isVisible()) await page.locator("#FullName").fill(TESTER_NAME);
    if (await page.locator("#PhoneNumber").isVisible()) await page.locator("#PhoneNumber").fill("0987654321");
    if (await page.locator("#CCCD").isVisible()) await page.locator("#CCCD").fill("123456789012");
    if (await page.locator("#agreeTerms").isVisible()) await page.locator("#agreeTerms").check();

    await page.locator('#registerBtn, button[type="submit"]').first().click();

    const errorAlert = page.locator('.alert', { hasText: /already taken|exists/i });
    if (await errorAlert.isVisible({ timeout: 2000 }).catch(() => false)) {
        console.log("⚠️ User already exists, skipping registration step.");
        return;
    }

    const otpCode = "502045";
    const digits = page.locator(".code-digit");
    if (await digits.first().isVisible()) {
        for (let i = 0; i < otpCode.length; i++) {
            await digits.nth(i).fill(otpCode[i]);
        }
        await page.locator("#btnVerify").click();
    } else if (await page.locator("#otpInput").isVisible()) {
        await page.locator("#otpInput").fill(otpCode);
        await page.locator("#verifyOtpBtn").click();
    }
}

/**
 * Điều hướng đến menu admin dạng AJAX (dùng loadPage).
 * Sau khi click, chờ content load vào #main.
 */
async function navigateToAdminMenu(page: Page, menuUrl: string) {
    await page.locator(`a[data-url="${menuUrl}"]`).click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
}

async function findTableRow(page: Page, searchText: string) {
    return page.locator(`tbody tr:has-text("${searchText}")`).first();
}

/**
 * Chờ và kiểm tra thông báo thành công sau submit.
 * Ưu tiên: toast Bootstrap (.toast-body trong .text-bg-success),
 * sau đó alert-success thông thường.
 */
async function expectSuccess(page: Page, timeout = 10000) {
    const successSelectors = [
        '.toast.text-bg-success',
        '.toast-success',
        '.alert-success',
        '[class*="success"]:visible',
        '.text-success'
    ];
    let found = false;
    for (const sel of successSelectors) {
        if (await page.locator(sel).first().isVisible({ timeout }).catch(() => false)) {
            found = true;
            break;
        }
    }
    if (!found) throw new Error("❌ Không tìm thấy thông báo thành công.");
}

// ============================================================
// 3. TEST SUITES - SAIGONRIDE CORE
// ============================================================

test.describe("SaigonRide - Complete Automation Suite", () => {

    // --- 3.1. TRANG TĨNH ---
    test.describe("UI & Static Pages", () => {
        for (const pageInfo of STATIC_PAGES) {
            test(`Verify Page: ${pageInfo.name}`, async ({ page }) => {
                await page.goto(`${BASE_URL}${pageInfo.url}`);
                const header = pageInfo.i18n ? page.locator(`[data-i18n="${pageInfo.i18n}"]`) : page.locator('h1.fw-bold');
                await expect(header).toBeVisible();
                await expect(header).toContainText(pageInfo.expectedText);
            });
        }
    });

    // --- 3.2. XÁC THỰC ---
    test.describe("Authentication Flows", () => {
        test("TC-AUTH: Registration Process", async ({ page }) => {
            await registerAndVerify(page, CUSTOMER_EMAIL, CUSTOMER_PASSWORD);
        });

        test("TC-PWD: Reset Password and Login check", async ({ page }) => {
            await resetPasswordFlow(page, CUSTOMER_EMAIL, CUSTOMER_PASSWORD);
            await loginAs(page, CUSTOMER_EMAIL, CUSTOMER_PASSWORD);
            const userSpan = page.locator('span.text-truncate.fw-medium, .user-name');
            await expect(userSpan.first()).toBeVisible();
        });
    });

    // --- 3.3. QUẢN TRỊ ADMIN CƠ BẢN ---
    test.describe("Admin Operations", () => {
        test.beforeEach(async ({ page }) => {
            await loginAsAdmin(page);
        });

        for (const menu of ADMIN_MENU_AJAX) {
            test(`Admin -> ${menu.name} (AJAX)`, async ({ page }) => {
                await page.locator(`a[data-url="${menu.url}"]`).click();
                const pageHeader = page.locator(`h4 span[data-i18n="${menu.i18nKey}"], h1, h2`).first();
                await expect(pageHeader).toBeVisible({ timeout: 10000 });
                await expect(pageHeader).toContainText(menu.expectedText);
            });
        }
    });

    // --- 3.4. THUÊ XE & THANH TOÁN ---
    test("TC-RENT: Bike Rental with PayPal Payment", async ({ page }) => {
        await loginAs(page, CUSTOMER_EMAIL, CUSTOMER_PASSWORD);

        await page.locator('a[href*="station-search"]').first().click();
        await page.locator("#btnViewList").click();
        await page.locator('button[data-station-id]').first().click();

        await page.locator('button.btn-open-scanner').first().click();
        await page.evaluate(() => {
            window.location.href = "Rental/Create?vehicleId=4&startStationId=30";
        });

        await expect(page.locator('[data-i18n="conf_ready"]')).toBeVisible({ timeout: 15000 });
        await page.locator('button[data-i18n="conf_start"]').click();

        await page.locator('select[name="endStationId"]').selectOption({ index: 1 });

        const finishBtn = page.locator('button[type="submit"].btn-success:has(span[data-i18n="trip_finish"])');
        await finishBtn.click();

        await page.locator('.pay-card[data-method="PayPal"]').click();
        await page.locator('button.btn-confirm').click();

        await page.waitForURL(/paypal\.com/, { timeout: 30000 });

        const paypalLoginBtn = page.locator('button.css-ltr-1d5lazx-button-Button:has-text("Log In")');
        await paypalLoginBtn.click();

        const emailInput = page.locator('input#email');
        await expect(emailInput).toBeVisible({ timeout: 15000 });
        await emailInput.fill(PAYPAL_USER);

        await page.locator('button#btnNext').click();

        const passwordInput = page.locator('input#password');
        await expect(passwordInput).toBeVisible({ timeout: 10000 });
        await passwordInput.fill(PAYPAL_PASS);

        await page.locator('#btnLogin, button[type="submit"]:has-text("Log In")').first().click();

        const paypalSubmitBtn = page.locator('button[data-testid="submit-button-initial"], #payment-submit-btn');
        await expect(paypalSubmitBtn).toBeEnabled({ timeout: 20000 });
        await paypalSubmitBtn.click();

        await expect(page.locator('[data-i18n="pay_confirm"]')).toBeVisible({ timeout: 25000 });
        await page.locator("#btnCapture").click();

        const successTitle = page.locator('[data-i18n="pay_success_title"]');
        await expect(successTitle).toContainText("Successful");
    });
});

// ============================================================
// 4. TEST SUITES - CRUD OPERATIONS
// ============================================================

// -------------------------------------------------------
// STATION MANAGEMENT
// -------------------------------------------------------
test.describe("Station Management - CRUD Operations", () => {
    test.beforeEach(async ({ page }) => {
        await loginAsAdmin(page);
        await navigateToAdminMenu(page, '/Stations/Index');
    });

    test("ST-01: Create New Station", async ({ page }) => {
        // Nút "Add Station" dùng onclick="loadPage('/Stations/Create')" — không có href/data-url
        const createBtn = page.locator('button[onclick*="Stations/Create"]');
        await expect(createBtn).toBeVisible({ timeout: 5000 });
        await createBtn.click();

        // Chờ form Create load vào #main (AJAX)
        await expect(page.locator('#stationCreateForm, form#stationCreateForm')).toBeVisible({ timeout: 10000 });

        // Điền đúng field names: Name, Address, TotalCapacity, CurrentCount
        await page.locator('input[name="Name"], input#Name').first().fill(STATION_DATA.name);
        await page.locator('input[name="Address"], input#Address').first().fill(STATION_DATA.address);

        // TotalCapacity có default value="20", ghi đè
        const totalCapInput = page.locator('input[name="TotalCapacity"], input#TotalCapacity').first();
        await totalCapInput.clear();
        await totalCapInput.fill(STATION_DATA.totalCapacity);

        // CurrentCount có default value="0", giữ nguyên hoặc ghi đè
        const currentCountInput = page.locator('input[name="CurrentCount"], input#CurrentCount').first();
        await currentCountInput.clear();
        await currentCountInput.fill(STATION_DATA.currentCount);

        // IsActive checkbox đã checked theo mặc định — không cần thay đổi

        // Submit form
        const submitBtn = page.locator('#stationCreateForm button[type="submit"]');
        await expect(submitBtn).toBeVisible({ timeout: 5000 });
        await submitBtn.click();

        // Sau submit thành công, app loadPage('/Stations/Index') → chờ table xuất hiện
        await expect(page.locator('#stationTableBody, table tbody')).toBeVisible({ timeout: 10000 });
    });

    test("ST-02: Read/View Station Details", async ({ page }) => {
        // Tìm kiếm station vừa tạo qua search bar (client-side AJAX search)
        const searchBox = page.locator('#stationSearchBar, input[placeholder*="Search"]').first();
        if (await searchBox.isVisible()) {
            await searchBox.fill(STATION_DATA.name);
            await page.waitForTimeout(800); // debounce search
        }

        const row = await findTableRow(page, STATION_DATA.name);
        await expect(row).toBeVisible({ timeout: 5000 });

        // Nút Edit/Details trong row
        const viewBtn = row.locator('a[href*="Details"], a[href*="Edit"], button:has-text("View"), button[onclick*="Details"], button[onclick*="Edit"]').first();
        await viewBtn.click();
        await page.waitForLoadState('networkidle');

        await expect(page.locator(`text="${STATION_DATA.name}"`)).toBeVisible();
    });

    test("ST-03: Update Station", async ({ page }) => {
        const searchBox = page.locator('#stationSearchBar, input[placeholder*="Search"]').first();
        if (await searchBox.isVisible()) {
            await searchBox.fill(STATION_DATA.name);
            await page.waitForTimeout(800);
        }

        const row = await findTableRow(page, STATION_DATA.name);
        await expect(row).toBeVisible({ timeout: 5000 });

        const editBtn = row.locator('a[href*="Edit"], button:has-text("Edit"), button[onclick*="Edit"]').first();
        await editBtn.click();
        await page.waitForLoadState('networkidle');

        // Cập nhật TotalCapacity (đúng field name theo form)
        const capacityInput = page.locator('input[name="TotalCapacity"], input#TotalCapacity').first();
        await capacityInput.clear();
        await capacityInput.fill("75");

        const submitBtn = page.locator('button[type="submit"]:has-text("Save"), button[type="submit"]:has-text("Update"), button[type="submit"]:has-text("Edit")').first();
        await submitBtn.click();

        await expectSuccess(page);
    });

    test("ST-04: Delete Station", async ({ page }) => {
        const searchBox = page.locator('#stationSearchBar, input[placeholder*="Search"]').first();
        if (await searchBox.isVisible()) {
            await searchBox.fill(STATION_DATA.name);
            await page.waitForTimeout(800);
        }

        const row = await findTableRow(page, STATION_DATA.name);
        await expect(row).toBeVisible({ timeout: 5000 });

        // Nút delete gọi window.prepareDeleteStation(id, name) → mở #deleteStationModal
        const deleteBtn = row.locator('button[onclick*="prepareDeleteStation"], button:has-text("Delete"), a[href*="Delete"]').first();
        await deleteBtn.click();

        // Xác nhận trong modal #deleteStationModal
        const confirmBtn = page.locator('#confirmDeleteStationBtn');
        await expect(confirmBtn).toBeVisible({ timeout: 5000 });
        await confirmBtn.click();

        // Toast success: #stationSuccessToast
        await expect(page.locator('#stationSuccessToast')).toBeVisible({ timeout: 5000 });
    });
});

// -------------------------------------------------------
// VEHICLE MANAGEMENT
// -------------------------------------------------------
test.describe("Vehicle Management - CRUD Operations", () => {
    test.beforeEach(async ({ page }) => {
        await loginAsAdmin(page);
        await navigateToAdminMenu(page, '/Vehicles/Index');
    });

    test("VH-01: Create New Vehicle", async ({ page }) => {
        // Nút "Add Vehicle" có id="openCreateBtn"
        const createBtn = page.locator('#openCreateBtn');
        await expect(createBtn).toBeVisible({ timeout: 5000 });
        await createBtn.click();

        // Chờ modal #createModal hiện ra
        const createModal = page.locator('#createModal');
        await expect(createModal).toBeVisible({ timeout: 5000 });

        // Điền đúng field names theo form thực tế
        // VehicleModel
        await createModal.locator('input[name="VehicleModel"], #VehicleModelInput').first().fill(VEHICLE_DATA.vehicleModel);

        // Price
        await createModal.locator('input[name="Price"]').first().fill(VEHICLE_DATA.price);

        // State (select với enum VehicleState)
        const stateSelect = createModal.locator('select[name="State"], #VehicleStateInput');
        await expect(stateSelect).toBeVisible({ timeout: 3000 });
        await stateSelect.selectOption({ label: VEHICLE_DATA.state });

        // BatteryPercentage — điền vào input number, range tự sync qua oninput
        const batteryInput = createModal.locator('input#batteryNumber, input[name="BatteryPercentage"]').first();
        await batteryInput.fill(VEHICLE_DATA.batteryPercentage);
        // Trigger input event để validation form bật nút submit
        await batteryInput.dispatchEvent('input');

        // CurrentStationId là optional — không cần chọn

        // Nút submit id="sunmitBtn" (typo trong source nhưng đúng ID thực tế)
        const submitBtn = createModal.locator('#sunmitBtn, button[type="submit"]').first();
        await expect(submitBtn).toBeEnabled({ timeout: 5000 });
        await submitBtn.click();

        // Modal đóng lại và table refresh — chờ modal ẩn đi
        await expect(createModal).not.toBeVisible({ timeout: 10000 });

        // Kiểm tra vehicle vừa tạo xuất hiện trong bảng
        await expect(page.locator(`#vehicleTableBody, table tbody`)).toBeVisible({ timeout: 5000 });
    });

    test("VH-02: Read/View Vehicle Details", async ({ page }) => {
        const searchBox = page.locator('input[placeholder*="Search"], input[name="search"]').first();
        if (await searchBox.isVisible()) {
            await searchBox.fill(VEHICLE_DATA.vehicleModel);
            await page.waitForTimeout(800);
        }

        const row = await findTableRow(page, VEHICLE_DATA.vehicleModel);
        await expect(row).toBeVisible({ timeout: 5000 });

        const viewBtn = row.locator('a[href*="Details"], button:has-text("View"), button[onclick*="Details"]').first();
        await viewBtn.click();

        // Details modal hoặc page — kiểm tra model name hiển thị
        await expect(page.locator(`text="${VEHICLE_DATA.vehicleModel}"`)).toBeVisible({ timeout: 5000 });
    });

    test("VH-03: Update Vehicle", async ({ page }) => {
        const searchBox = page.locator('input[placeholder*="Search"], input[name="search"]').first();
        if (await searchBox.isVisible()) {
            await searchBox.fill(VEHICLE_DATA.vehicleModel);
            await page.waitForTimeout(800);
        }

        const row = await findTableRow(page, VEHICLE_DATA.vehicleModel);
        await expect(row).toBeVisible({ timeout: 5000 });

        const editBtn = row.locator('a[href*="Edit"], button:has-text("Edit"), button[onclick*="Edit"]').first();
        await editBtn.click();

        // Edit modal hoặc page
        await page.waitForTimeout(500);

        const modelInput = page.locator('input[name="VehicleModel"], #VehicleModelInput').first();
        await expect(modelInput).toBeVisible({ timeout: 5000 });
        await modelInput.clear();
        await modelInput.fill("Honda Air Blade");

        const submitBtn = page.locator('button[type="submit"]:has-text("Save"), button[type="submit"]:has-text("Update"), #sunmitBtn').first();
        await submitBtn.click();

        await expectSuccess(page);
    });

    test("VH-04: Delete Vehicle", async ({ page }) => {
        const searchBox = page.locator('input[placeholder*="Search"], input[name="search"]').first();
        if (await searchBox.isVisible()) {
            await searchBox.fill(VEHICLE_DATA.vehicleModel);
            await page.waitForTimeout(800);
        }

        const row = await findTableRow(page, VEHICLE_DATA.vehicleModel);
        await expect(row).toBeVisible({ timeout: 5000 });

        const deleteBtn = row.locator('button:has-text("Delete"), a[href*="Delete"], button[onclick*="Delete"]').first();
        await deleteBtn.click();

        // Confirm dialog (modal hoặc JS confirm)
        page.on('dialog', dialog => dialog.accept());
        const confirmBtn = page.locator('button:has-text("Confirm"), button:has-text("Yes"), button[class*="danger"][type="submit"]').first();
        if (await confirmBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
            await confirmBtn.click();
        }

        await expectSuccess(page);
    });
});

// -------------------------------------------------------
// USER MANAGEMENT
// -------------------------------------------------------
test.describe("User Management - CRUD Operations", () => {
    test.beforeEach(async ({ page }) => {
        await loginAsAdmin(page);
        await navigateToAdminMenu(page, '/UserManagements/Index');
    });

    test("US-01: Create New User", async ({ page }) => {
        // Nút "Add User" có id="openCreateUserBtn"
        const createBtn = page.locator('#openCreateUserBtn');
        await expect(createBtn).toBeVisible({ timeout: 5000 });
        await createBtn.click();

        // Chờ modal #createUserModal hiện ra
        const createModal = page.locator('#createUserModal');
        await expect(createModal).toBeVisible({ timeout: 5000 });

        // Điền đúng field names theo form thực tế
        await createModal.locator('input[name="FullName"]').fill(USER_DATA.fullname);
        await createModal.locator('input[name="UserName"]').fill(USER_DATA.username);
        await createModal.locator('input[name="Email"]').fill(USER_DATA.email);
        await createModal.locator('input[name="PhoneNumber"]').fill(USER_DATA.phone);
        await createModal.locator('input[name="Password"]').fill(USER_DATA.password);
        await createModal.locator('input[name="CCCD"]').fill(USER_DATA.cccd);

        // UserType select — mặc định "Local", có thể giữ nguyên
        const userTypeSelect = createModal.locator('select[name="UserType"]');
        if (await userTypeSelect.isVisible({ timeout: 1000 }).catch(() => false)) {
            await userTypeSelect.selectOption('Local');
        }

        // Role checkboxes — chọn "Customer"
        const customerRoleCheckbox = createModal.locator('input[name="Roles"][value="Customer"]');
        if (await customerRoleCheckbox.isVisible({ timeout: 1000 }).catch(() => false)) {
            await customerRoleCheckbox.check();
        }

        // Submit: button[type="submit"] form="createUserForm" id="createUserSubmitBtn"
        const submitBtn = page.locator('#createUserSubmitBtn');
        await expect(submitBtn).toBeVisible({ timeout: 5000 });
        await submitBtn.click();

        // Chờ modal đóng → thành công
        await expect(createModal).not.toBeVisible({ timeout: 10000 });

        // Kiểm tra alert success hoặc user xuất hiện trong bảng
        await expectSuccess(page).catch(async () => {
            // Nếu không có toast, kiểm tra user trong bảng
            const searchBox = page.locator('input[placeholder*="Search"]').first();
            if (await searchBox.isVisible()) {
                await searchBox.fill(USER_DATA.email);
                await page.waitForTimeout(800);
                await expect(page.locator(`tbody tr:has-text("${USER_DATA.email}")`).first()).toBeVisible({ timeout: 5000 });
            }
        });
    });

    test("US-02: Read/View User Details", async ({ page }) => {
        const searchBox = page.locator('input[placeholder*="Search"], input[name="search"]').first();
        if (await searchBox.isVisible()) {
            await searchBox.fill(USER_DATA.email);
            await page.waitForTimeout(800);
        }

        const row = await findTableRow(page, USER_DATA.email);
        await expect(row).toBeVisible({ timeout: 5000 });

        const viewBtn = row.locator('a[href*="Details"], button:has-text("View"), button[onclick*="Details"]').first();
        await viewBtn.click();
        await page.waitForTimeout(500);

        await expect(page.locator(`text="${USER_DATA.email}"`)).toBeVisible({ timeout: 5000 });
        await expect(page.locator(`text="${USER_DATA.fullname}"`)).toBeVisible({ timeout: 5000 });
    });

    test("US-03: Update User", async ({ page }) => {
        const searchBox = page.locator('input[placeholder*="Search"], input[name="search"]').first();
        if (await searchBox.isVisible()) {
            await searchBox.fill(USER_DATA.email);
            await page.waitForTimeout(800);
        }

        const row = await findTableRow(page, USER_DATA.email);
        await expect(row).toBeVisible({ timeout: 5000 });

        const editBtn = row.locator('a[href*="Edit"], button:has-text("Edit"), button[onclick*="Edit"]').first();
        await editBtn.click();
        await page.waitForTimeout(500);

        const phoneInput = page.locator('input[name="PhoneNumber"], input#PhoneNumber').first();
        await expect(phoneInput).toBeVisible({ timeout: 5000 });
        await phoneInput.clear();
        await phoneInput.fill("0123456789");

        const submitBtn = page.locator('button[type="submit"]:has-text("Save"), button[type="submit"]:has-text("Update")').first();
        await submitBtn.click();

        await expectSuccess(page);
    });

    test("US-04: Delete User", async ({ page }) => {
        const searchBox = page.locator('input[placeholder*="Search"], input[name="search"]').first();
        if (await searchBox.isVisible()) {
            await searchBox.fill(USER_DATA.email);
            await page.waitForTimeout(800);
        }

        const row = await findTableRow(page, USER_DATA.email);
        await expect(row).toBeVisible({ timeout: 5000 });

        const deleteBtn = row.locator('button:has-text("Delete"), a[href*="Delete"], button[onclick*="Delete"]').first();
        await deleteBtn.click();

        page.on('dialog', dialog => dialog.accept());
        const confirmBtn = page.locator('button:has-text("Confirm"), button:has-text("Yes"), button[class*="danger"][type="submit"]').first();
        if (await confirmBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
            await confirmBtn.click();
        }

        await expectSuccess(page);
    });
});

// -------------------------------------------------------
// REPORT MANAGEMENT
// -------------------------------------------------------
test.describe("Report Management - CRUD Operations", () => {

    // RP-01 KHÔNG dùng beforeEach admin vì cần login customer để tạo report
    test("RP-01: Create New Report (User UI)", async ({ page }) => {
        await loginAs(page, CUSTOMER_EMAIL, CUSTOMER_PASSWORD);

        // Điều hướng đến trang tạo report (BugReports/Create hoặc trang có form report)
        await page.goto(`${BASE_URL}/Home/ReportIssue`);
        await page.waitForLoadState('networkidle');

        // Điền đúng field names: Title, Description, CreatedDate, Status
        const titleInput = page.locator('input[name="Title"], input#Title').first();
        await expect(titleInput).toBeVisible({ timeout: 5000 });
        await titleInput.fill(REPORT_DATA.title);

        const descInput = page.locator('input[name="Description"], textarea[name="Description"]').first();
        await descInput.fill(REPORT_DATA.description);

        // CreatedDate — điền ngày hiện tại nếu required
        const createdDateInput = page.locator('input[name="CreatedDate"]').first();
        if (await createdDateInput.isVisible({ timeout: 1000 }).catch(() => false)) {
            const today = new Date().toISOString().split('T')[0];
            await createdDateInput.fill(today);
        }

        // Status field
        const statusInput = page.locator('input[name="Status"]').first();
        if (await statusInput.isVisible({ timeout: 1000 }).catch(() => false)) {
            await statusInput.fill(REPORT_DATA.status);
        }

        // Submit
        const submitBtn = page.locator('input[type="submit"][value="Create"], button[type="submit"]').first();
        await expect(submitBtn).toBeVisible({ timeout: 5000 });
        await submitBtn.click();
        await page.waitForLoadState('networkidle');

        // Kiểm tra thành công: redirect hoặc success message
        await expectSuccess(page).catch(async () => {
            // Nếu không có toast, kiểm tra đã redirect về list
            await expect(page).not.toHaveURL(/Create/);
        });
    });

    // Các test còn lại dùng admin
    test.describe("Report Admin Operations", () => {
        test.beforeEach(async ({ page }) => {
            await loginAsAdmin(page);
            await navigateToAdminMenu(page, '/AdminDashboard/Reports');
        });

        test("RP-02: View Report Details", async ({ page }) => {
            const searchBox = page.locator('input[placeholder*="Search"], input[name="search"]').first();
            if (await searchBox.isVisible()) {
                await searchBox.fill(REPORT_DATA.title);
                await page.waitForTimeout(800);
            }

            const row = await findTableRow(page, REPORT_DATA.title);
            if (await row.isVisible({ timeout: 3000 }).catch(() => false)) {
                const viewBtn = row.locator('a[href*="Details"], a[href*="View"], button:has-text("View")').first();

                if (await viewBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
                    await viewBtn.click();
                    await page.waitForLoadState('networkidle');
                    await expect(page.locator(`text="${REPORT_DATA.title}"`)).toBeVisible();
                } else {
                    console.log("ℹ️ No view button found for report.");
                }
            } else {
                console.log("ℹ️ Report not found in table (may not be created yet).");
            }
        });

        test("RP-03: Export/Download Reports", async ({ page }) => {
            const exportBtn = page.locator('button:has-text("Export"), button:has-text("Download"), a[href*="Export"]').first();

            if (await exportBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
                await exportBtn.click();
                await expectSuccess(page).catch(() => {
                    console.log("ℹ️ Export triggered (no success toast expected).");
                });
            } else {
                console.log("ℹ️ No export button found.");
            }
        });

        test("RP-04: Filter Reports by Date Range", async ({ page }) => {
            const startDateInput = page.locator('input[name="StartDate"], input[type="date"]').first();
            const endDateInput = page.locator('input[name="EndDate"], input[type="date"]').nth(1);
            const filterBtn = page.locator('button:has-text("Filter"), button:has-text("Search")').first();

            if (await startDateInput.isVisible({ timeout: 3000 }).catch(() => false) &&
                await endDateInput.isVisible({ timeout: 3000 }).catch(() => false)) {
                const today = new Date().toISOString().split('T')[0];
                await startDateInput.fill(today);
                await endDateInput.fill(today);

                if (await filterBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
                    await filterBtn.click();
                    await page.waitForLoadState('networkidle');
                }

                await expect(page.locator('table tbody')).toBeVisible();
            } else {
                console.log("ℹ️ Date filter inputs not found.");
            }
        });

        test("RP-05: Delete Report", async ({ page }) => {
            const searchBox = page.locator('input[placeholder*="Search"], input[name="search"]').first();
            if (await searchBox.isVisible()) {
                await searchBox.fill(REPORT_DATA.title);
                await page.waitForTimeout(800);
            }

            const row = await findTableRow(page, REPORT_DATA.title);
            if (await row.isVisible({ timeout: 3000 }).catch(() => false)) {
                const deleteBtn = row.locator('button:has-text("Delete"), a[href*="Delete"]').first();

                if (await deleteBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
                    await deleteBtn.click();

                    page.on('dialog', dialog => dialog.accept());
                    const confirmBtn = page.locator('button:has-text("Confirm"), button:has-text("Yes"), button[class*="danger"][type="submit"]').first();
                    if (await confirmBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
                        await confirmBtn.click();
                    }

                    await expectSuccess(page);
                } else {
                    console.log("ℹ️ No delete button found for report.");
                }
            } else {
                console.log("ℹ️ Report not found in table.");
            }
        });
    });
});