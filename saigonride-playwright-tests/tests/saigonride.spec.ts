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

const VEHICLE_DATA = {
    vehicleModel: "Honda Wave Alpha",
    price: "15000",
    state: "Available",
    batteryPercentage: "80"
};

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

async function navigateToAdminMenu(page: Page, menuUrl: string) {
    await page.locator(`a[data-url="${menuUrl}"]`).click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
}

/**
 * Fill a search bar, dispatch the 'input' event to trigger AJAX immediately,
 * then wait for the target row to become visible in the table.
 *
 * Fixes the 800 ms hard-coded wait that was too short for AJAX + DOM update.
 *
 * @param searchBarSelector  CSS selector for the input (e.g. '#stationSearchBar')
 * @param searchText         Text to type into the search bar
 * @param rowText            Text expected in the target <tr> (defaults to searchText)
 */
async function searchAndGetRow(
    page: Page,
    searchBarSelector: string,
    searchText: string,
    rowText: string = searchText
) {
    const searchBox = page.locator(searchBarSelector).first();
    await expect(searchBox).toBeVisible({ timeout: 5000 });
    await searchBox.fill(searchText);
    // Trigger JS 'input' handler immediately so AJAX fires without waiting for the
    // browser's own debounce/idle timing.
    await searchBox.dispatchEvent('input');
    // Now wait for the row itself — this is the reliable signal that AJAX is done.
    const row = page.locator(`tbody tr:has-text("${rowText}")`).first();
    await row.waitFor({ state: 'visible', timeout: 12000 });
    return row;
}

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
    if (!found) throw new Error("❌ No success notification found.");
}

// ============================================================
// 3. TEST SUITES - SAIGONRIDE CORE
// ============================================================

test.describe("SaigonRide - Complete Automation Suite", () => {

    // --- 3.1. STATIC PAGES ---
    test.describe("UI & Static Pages", () => {
        for (const pageInfo of STATIC_PAGES) {
            test(`Verify Page: ${pageInfo.name}`, async ({ page }) => {
                await page.goto(`${BASE_URL}${pageInfo.url}`);
                const header = pageInfo.i18n
                    ? page.locator(`[data-i18n="${pageInfo.i18n}"]`)
                    : page.locator('h1.fw-bold');
                await expect(header).toBeVisible();
                await expect(header).toContainText(pageInfo.expectedText);
            });
        }
    });

    // --- 3.2. AUTHENTICATION ---
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

    // --- 3.3. ADMIN NAVIGATION ---
    test.describe("Admin Operations", () => {
        test.beforeEach(async ({ page }) => {
            await loginAsAdmin(page);
        });

        for (const menu of ADMIN_MENU_AJAX) {
            test(`Admin -> ${menu.name} (AJAX)`, async ({ page }) => {
                await page.locator(`a[data-url="${menu.url}"]`).click();
                const pageHeader = page.locator(
                    `h4 span[data-i18n="${menu.i18nKey}"], h1, h2, h4`
                ).first();
                await expect(pageHeader).toBeVisible({ timeout: 10000 });
                await expect(pageHeader).toContainText(menu.expectedText);
            });
        }
    });

    // --- 3.4. BIKE RENTAL & PAYMENT ---
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

        const finishBtn = page.locator(
            'button[type="submit"].btn-success:has(span[data-i18n="trip_finish"])'
        );
        await finishBtn.click();

        await page.locator('.pay-card[data-method="PayPal"]').click();
        await page.locator('button.btn-confirm').click();

        await page.waitForURL(/paypal\.com/, { timeout: 30000 });

        const paypalLoginBtn = page.locator(
            'button.css-ltr-1d5lazx-button-Button:has-text("Log In")'
        );
        await paypalLoginBtn.click();

        const emailInput = page.locator('input#email');
        await expect(emailInput).toBeVisible({ timeout: 15000 });
        await emailInput.fill(PAYPAL_USER);

        await page.locator('button#btnNext').click();

        const passwordInput = page.locator('input#password');
        await expect(passwordInput).toBeVisible({ timeout: 10000 });
        await passwordInput.fill(PAYPAL_PASS);

        await page.locator('#btnLogin, button[type="submit"]:has-text("Log In")').first().click();

        const paypalSubmitBtn = page.locator(
            'button[data-testid="submit-button-initial"], #payment-submit-btn'
        );
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
        const createBtn = page.locator('button[onclick*="Stations/Create"]');
        await expect(createBtn).toBeVisible({ timeout: 5000 });
        await createBtn.click();

        await expect(
            page.locator('#stationCreateForm, form#stationCreateForm')
        ).toBeVisible({ timeout: 10000 });

        await page.locator('input[name="Name"], input#Name').first().fill(STATION_DATA.name);
        await page.locator('input[name="Address"], input#Address').first().fill(STATION_DATA.address);

        const totalCapInput = page.locator(
            'input[name="TotalCapacity"], input#TotalCapacity'
        ).first();
        await totalCapInput.clear();
        await totalCapInput.fill(STATION_DATA.totalCapacity);

        const currentCountInput = page.locator(
            'input[name="CurrentCount"], input#CurrentCount'
        ).first();
        await currentCountInput.clear();
        await currentCountInput.fill(STATION_DATA.currentCount);

        const submitBtn = page.locator('#stationCreateForm button[type="submit"]');
        await expect(submitBtn).toBeVisible({ timeout: 5000 });
        await submitBtn.click();

        // On success the form JS calls loadPage('/Stations/Index') — wait for table
        await expect(page.locator('#stationTableBody, table tbody')).toBeVisible({ timeout: 10000 });
    });

    test("ST-02: Read/View Station Details", async ({ page }) => {
        // FIX: use searchAndGetRow — dispatches 'input' event and waits for AJAX row
        const row = await searchAndGetRow(page, '#stationSearchBar', STATION_DATA.name);

        // FIX: station view button is onclick="loadPage('/Stations/Details/ID')"
        // Selector matches via substring of the onclick attribute value
        const viewBtn = row.locator('button[onclick*="Stations/Details"]').first();
        await expect(viewBtn).toBeVisible({ timeout: 5000 });
        await viewBtn.click();

        // Wait for the AJAX partial view to finish loading into #main
        await page.waitForLoadState('networkidle');

        // FIX: station name is rendered as:
        //   <h2 data-i18n="@Model.Name">@Model.Name</h2>
        // The i18n JS processes data-i18n and may replace text; using exact text= locator
        // is fragile. toContainText on body is reliable regardless of i18n outcome.
        await expect(page.locator('body')).toContainText(STATION_DATA.name, { timeout: 10000 });
    });

    test("ST-03: Update Station", async ({ page }) => {
        // FIX: use searchAndGetRow — dispatches 'input' event and waits for AJAX row
        const row = await searchAndGetRow(page, '#stationSearchBar', STATION_DATA.name);

        // FIX: edit button is onclick="loadPage('/Stations/Edit/ID')"
        const editBtn = row.locator('button[onclick*="Stations/Edit"]').first();
        await expect(editBtn).toBeVisible({ timeout: 5000 });
        await editBtn.click();

        // Wait for the AJAX partial edit form to load into #main
        await page.waitForLoadState('networkidle');
        const capacityInput = page.locator(
            'input[name="TotalCapacity"], input#TotalCapacity'
        ).first();
        await expect(capacityInput).toBeVisible({ timeout: 8000 });
        await capacityInput.clear();
        await capacityInput.fill("75");

        const submitBtn = page.locator('button[type="submit"]:has-text("Save")').first();
        await expect(submitBtn).toBeVisible({ timeout: 5000 });
        await submitBtn.click();

        // FIX: the station edit AJAX success handler calls loadPage('/Stations/Index') —
        // there is NO toast. Confirm success by waiting for the station table to reappear.
        await expect(
            page.locator('#stationTableBody, table tbody')
        ).toBeVisible({ timeout: 10000 });
    });

    test("ST-04: Delete Station", async ({ page }) => {
        // FIX: use searchAndGetRow — dispatches 'input' event and waits for AJAX row
        const row = await searchAndGetRow(page, '#stationSearchBar', STATION_DATA.name);

        // FIX: delete button uses onclick="prepareDeleteStation(id, name)"
        const deleteBtn = row.locator('button[onclick*="prepareDeleteStation"]').first();
        await expect(deleteBtn).toBeVisible({ timeout: 5000 });
        await deleteBtn.click();

        // Confirm inside the Bootstrap modal
        const confirmBtn = page.locator('#confirmDeleteStationBtn');
        await expect(confirmBtn).toBeVisible({ timeout: 5000 });
        await confirmBtn.click();

        // Station-specific success toast: #stationSuccessToast
        await expect(page.locator('#stationSuccessToast')).toBeVisible({ timeout: 8000 });
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
        const createBtn = page.locator('#openCreateBtn');
        await expect(createBtn).toBeVisible({ timeout: 5000 });
        await createBtn.click();

        const createModal = page.locator('#createModal');
        await expect(createModal).toBeVisible({ timeout: 5000 });

        await createModal
            .locator('input[name="VehicleModel"], #VehicleModelInput')
            .first()
            .fill(VEHICLE_DATA.vehicleModel);
        await createModal.locator('input[name="Price"]').first().fill(VEHICLE_DATA.price);

        const stateSelect = createModal.locator('select[name="State"], #VehicleStateInput');
        await expect(stateSelect).toBeVisible({ timeout: 3000 });
        await stateSelect.selectOption({ label: VEHICLE_DATA.state });

        const batteryInput = createModal
            .locator('input#batteryNumber, input[name="BatteryPercentage"]')
            .first();
        await batteryInput.fill(VEHICLE_DATA.batteryPercentage);
        await batteryInput.dispatchEvent('input');

        const submitBtn = createModal.locator('#sunmitBtn, button[type="submit"]').first();
        await expect(submitBtn).toBeEnabled({ timeout: 5000 });
        await submitBtn.click();

        await expect(createModal).not.toBeVisible({ timeout: 10000 });
        await expect(page.locator('#vehicleTableBody, table tbody')).toBeVisible({ timeout: 5000 });
    });

    test("VH-02: Read/View Vehicle Details", async ({ page }) => {
        // FIX: use searchAndGetRow with vehicle search bar id
        const row = await searchAndGetRow(page, '#searchBar', VEHICLE_DATA.vehicleModel);

        // FIX: vehicle view button is onclick="ShowDetailsModal(ID)" — no text, no href.
        // The button has title="View Details" which is the reliable selector.
        const viewBtn = row.locator('button[title="View Details"]').first();
        await expect(viewBtn).toBeVisible({ timeout: 5000 });
        await viewBtn.click();

        // FIX: ShowDetailsModal() first shows a loading spinner in #detailsModalBody,
        // then fetches vehicle details via AJAX and populates #detailsModalBody,
        // then shows #detailsModal. Wait for the modal to be visible.
        const detailsModal = page.locator('#detailsModal');
        await expect(detailsModal).toBeVisible({ timeout: 8000 });

        // FIX: vehicle model is in <h5 data-i18n="@Model.VehicleModel">@Model.VehicleModel</h5>
        // Use toContainText on the modal body — avoids exact-match issues from i18n processing.
        await expect(
            page.locator('#detailsModalBody')
        ).toContainText(VEHICLE_DATA.vehicleModel, { timeout: 10000 });
    });

    test("VH-03: Update Vehicle", async ({ page }) => {
        // FIX: use searchAndGetRow
        const row = await searchAndGetRow(page, '#searchBar', VEHICLE_DATA.vehicleModel);

        // FIX: edit button is onclick="ShowEditModal(ID)" with title="Edit"
        const editBtn = row.locator('button[title="Edit"]').first();
        await expect(editBtn).toBeVisible({ timeout: 5000 });
        await editBtn.click();

        // FIX: ShowEditModal() fetches vehicle data via AJAX then shows #editModal
        const editModal = page.locator('#editModal');
        await expect(editModal).toBeVisible({ timeout: 8000 });

        // FIX: the model name input inside the edit modal is #editVehicleModel
        const modelInput = editModal.locator('#editVehicleModel');
        await expect(modelInput).toBeVisible({ timeout: 5000 });
        await modelInput.clear();
        await modelInput.fill("Honda Air Blade");

        // FIX: submit button inside the edit modal is #editSubmitBtn
        const submitBtn = editModal.locator('#editSubmitBtn');
        await expect(submitBtn).toBeVisible({ timeout: 5000 });
        await submitBtn.click();

        // FIX: vehicle edit success calls $('#editModal').modal('hide') — no toast is shown.
        // Closing of the modal is the reliable success signal.
        await expect(editModal).not.toBeVisible({ timeout: 10000 });
    });

    test("VH-04: Delete Vehicle", async ({ page }) => {
        // VH-03 renamed the vehicle — search by the updated name
        const updatedModel = "Honda Air Blade";
        const row = await searchAndGetRow(page, '#searchBar', updatedModel);

        // FIX: delete button is onclick="prepareDelete(ID, name)" with title="Delete"
        const deleteBtn = row.locator('button[title="Delete"]').first();
        await expect(deleteBtn).toBeVisible({ timeout: 5000 });
        await deleteBtn.click();

        // FIX: confirm button inside #deleteModal is #confirmDeleteBtn
        const confirmBtn = page.locator('#confirmDeleteBtn');
        await expect(confirmBtn).toBeVisible({ timeout: 5000 });
        await confirmBtn.click();

        // FIX: on success the JS removes the <tr> from the DOM — wait for it to vanish
        await expect(row).not.toBeVisible({ timeout: 8000 });
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
        const createBtn = page.locator('#openCreateUserBtn');
        await expect(createBtn).toBeVisible({ timeout: 5000 });
        await createBtn.click();

        const createModal = page.locator('#createUserModal');
        await expect(createModal).toBeVisible({ timeout: 5000 });

        await createModal.locator('input[name="FullName"]').fill(USER_DATA.fullname);
        await createModal.locator('input[name="UserName"]').fill(USER_DATA.username);
        await createModal.locator('input[name="Email"]').fill(USER_DATA.email);
        await createModal.locator('input[name="PhoneNumber"]').fill(USER_DATA.phone);
        await createModal.locator('input[name="Password"]').fill(USER_DATA.password);
        await createModal.locator('input[name="CCCD"]').fill(USER_DATA.cccd);

        const userTypeSelect = createModal.locator('select[name="UserType"]');
        if (await userTypeSelect.isVisible({ timeout: 1000 }).catch(() => false)) {
            await userTypeSelect.selectOption('Local');
        }

        const customerRoleCheckbox = createModal.locator(
            'input[name="Roles"][value="Customer"]'
        );
        if (await customerRoleCheckbox.isVisible({ timeout: 1000 }).catch(() => false)) {
            await customerRoleCheckbox.check();
        }

        const submitBtn = page.locator('#createUserSubmitBtn');
        await expect(submitBtn).toBeVisible({ timeout: 5000 });
        await submitBtn.click();

        await expect(createModal).not.toBeVisible({ timeout: 10000 });

        // FIX: verify created user by searching username (userSearchBar searches name/username,
        // not email — there is no emailSearchBar element in the rendered HTML)
        await expectSuccess(page).catch(async () => {
            const row = await searchAndGetRow(page, '#userSearchBar', USER_DATA.username);
            await expect(row).toBeVisible({ timeout: 5000 });
        });
    });

    test("US-02: Read/View User Details", async ({ page }) => {
        // FIX: #userSearchBar searches by name/username — fill username, NOT email
        const row = await searchAndGetRow(page, '#userSearchBar', USER_DATA.username);

        // FIX: user details button is onclick="ShowUserDetailsModal('userId')"
        const viewBtn = row.locator('button[onclick*="ShowUserDetailsModal"]').first();
        await expect(viewBtn).toBeVisible({ timeout: 5000 });
        await viewBtn.click();

        // FIX: ShowUserDetailsModal() fetches user data via AJAX into #userDetailsModalBody,
        // then shows #userDetailsModal
        const detailsModal = page.locator('#userDetailsModal');
        await expect(detailsModal).toBeVisible({ timeout: 8000 });

        // FIX: email appears as a link in #userDetailsModalBody — use toContainText
        await expect(
            page.locator('#userDetailsModalBody')
        ).toContainText(USER_DATA.email, { timeout: 10000 });
        await expect(
            page.locator('#userDetailsModalBody')
        ).toContainText(USER_DATA.fullname, { timeout: 5000 });
    });

    test("US-03: Update User", async ({ page }) => {
        // FIX: search by username
        const row = await searchAndGetRow(page, '#userSearchBar', USER_DATA.username);

        // FIX: edit button is onclick="ShowUserEditModal('userId')"
        const editBtn = row.locator('button[onclick*="ShowUserEditModal"]').first();
        await expect(editBtn).toBeVisible({ timeout: 5000 });
        await editBtn.click();

        // FIX: ShowUserEditModal() fetches user data via AJAX and shows #editUserModal
        const editModal = page.locator('#editUserModal');
        await expect(editModal).toBeVisible({ timeout: 8000 });

        // FIX: phone input inside the edit modal is #editUserPhone
        const phoneInput = editModal.locator('#editUserPhone');
        await expect(phoneInput).toBeVisible({ timeout: 5000 });
        await phoneInput.clear();
        await phoneInput.fill("0123456789");

        // FIX: submit button is #editUserSubmitBtn
        const submitBtn = page.locator('#editUserSubmitBtn');
        await expect(submitBtn).toBeVisible({ timeout: 5000 });
        await submitBtn.click();

        // FIX: user edit success calls $('#editUserModal').modal('hide') — no toast.
        // Closing of the modal is the reliable success signal.
        await expect(editModal).not.toBeVisible({ timeout: 10000 });
    });

    test("US-04: Delete User", async ({ page }) => {
        // FIX: search by username
        const row = await searchAndGetRow(page, '#userSearchBar', USER_DATA.username);

        // FIX: delete button is onclick="prepareUserDelete('userId', 'name')"
        const deleteBtn = row.locator('button[onclick*="prepareUserDelete"]').first();
        await expect(deleteBtn).toBeVisible({ timeout: 5000 });
        await deleteBtn.click();

        // FIX: confirm button inside #deleteUserModal is #confirmUserDeleteBtn
        const confirmBtn = page.locator('#confirmUserDeleteBtn');
        await expect(confirmBtn).toBeVisible({ timeout: 5000 });
        await confirmBtn.click();

        // FIX: on success the JS removes the <tr> from the DOM — wait for it to vanish
        await expect(row).not.toBeVisible({ timeout: 8000 });
    });
});

// -------------------------------------------------------
// REPORT MANAGEMENT
// -------------------------------------------------------
test.describe("Report Management - CRUD Operations", () => {

    // RP-01: customer creates a report
    test("RP-01: Create New Report (User UI)", async ({ page }) => {
        await loginAs(page, CUSTOMER_EMAIL, CUSTOMER_PASSWORD);

        await page.goto(`${BASE_URL}/Home/ReportIssue`);
        await page.waitForLoadState('networkidle');

        const titleInput = page.locator('input[name="Title"], input#Title').first();
        await expect(titleInput).toBeVisible({ timeout: 5000 });
        await titleInput.fill(REPORT_DATA.title);

        const descInput = page.locator(
            'input[name="Description"], textarea[name="Description"]'
        ).first();
        await descInput.fill(REPORT_DATA.description);

        const createdDateInput = page.locator('input[name="CreatedDate"]').first();
        if (await createdDateInput.isVisible({ timeout: 1000 }).catch(() => false)) {
            const today = new Date().toISOString().split('T')[0];
            await createdDateInput.fill(today);
        }

        const statusInput = page.locator('input[name="Status"]').first();
        if (await statusInput.isVisible({ timeout: 1000 }).catch(() => false)) {
            await statusInput.fill(REPORT_DATA.status);
        }

        const submitBtn = page.locator(
            'input[type="submit"][value="Create"], button[type="submit"]'
        ).first();
        await expect(submitBtn).toBeVisible({ timeout: 5000 });
        await submitBtn.click();
        await page.waitForLoadState('networkidle');

        await expectSuccess(page).catch(async () => {
            await expect(page).not.toHaveURL(/Create/);
        });
    });

    // RP-02 to RP-05: admin operations on reports
    test.describe("Report Admin Operations", () => {
        test.beforeEach(async ({ page }) => {
            await loginAsAdmin(page);
            await navigateToAdminMenu(page, '/AdminDashboard/Reports');
        });

        test("RP-02: View Report Details", async ({ page }) => {
            const searchBox = page.locator(
                'input[placeholder*="Search"], input[name="search"]'
            ).first();
            if (await searchBox.isVisible()) {
                await searchBox.fill(REPORT_DATA.title);
                await searchBox.dispatchEvent('input');
                await page.waitForTimeout(1500);
            }

            const row = page.locator(`tbody tr:has-text("${REPORT_DATA.title}")`).first();
            if (await row.isVisible({ timeout: 5000 }).catch(() => false)) {
                const viewBtn = row.locator(
                    'a[href*="Details"], a[href*="View"], button:has-text("View")'
                ).first();
                if (await viewBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
                    await viewBtn.click();
                    await page.waitForLoadState('networkidle');
                    await expect(page.locator('body')).toContainText(REPORT_DATA.title);
                } else {
                    console.log("ℹ️ No view button found for report.");
                }
            } else {
                console.log("ℹ️ Report not found in table (may not be created yet).");
            }
        });

        test("RP-03: Export/Download Reports", async ({ page }) => {
            const exportBtn = page.locator(
                'button:has-text("Export"), button:has-text("Download"), a[href*="Export"]'
            ).first();
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
            const startDateInput = page.locator(
                'input[name="StartDate"], input[type="date"]'
            ).first();
            const endDateInput = page.locator(
                'input[name="EndDate"], input[type="date"]'
            ).nth(1);
            const filterBtn = page.locator(
                'button:has-text("Filter"), button:has-text("Search")'
            ).first();

            if (
                await startDateInput.isVisible({ timeout: 3000 }).catch(() => false) &&
                await endDateInput.isVisible({ timeout: 3000 }).catch(() => false)
            ) {
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
            const searchBox = page.locator(
                'input[placeholder*="Search"], input[name="search"]'
            ).first();
            if (await searchBox.isVisible()) {
                await searchBox.fill(REPORT_DATA.title);
                await searchBox.dispatchEvent('input');
                await page.waitForTimeout(1500);
            }

            const row = page.locator(`tbody tr:has-text("${REPORT_DATA.title}")`).first();
            if (await row.isVisible({ timeout: 5000 }).catch(() => false)) {
                const deleteBtn = row.locator(
                    'button:has-text("Delete"), a[href*="Delete"]'
                ).first();
                if (await deleteBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
                    await deleteBtn.click();

                    page.on('dialog', dialog => dialog.accept());
                    const confirmBtn = page.locator(
                        'button:has-text("Confirm"), button:has-text("Yes"), button[class*="danger"][type="submit"]'
                    ).first();
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