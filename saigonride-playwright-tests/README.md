# SaiGonRide – Playwright Automation Tests

## Cài đặt

```bash
# Tạo thư mục riêng cho test (hoặc để trong thư mục gốc project)
mkdir saigonride-e2e && cd saigonride-e2e

# Copy 2 file vào đây:
#   saigonride.spec.ts
#   playwright.config.ts

# Cài Playwright
npm init -y
npm install -D @playwright/test
npx playwright install chromium
```

## Chạy test

```bash
# Chạy toàn bộ (hiện cửa sổ trình duyệt)
npx playwright test

# Chạy 1 suite cụ thể
npx playwright test --grep "TC-V"        # Vehicle tests
npx playwright test --grep "TC-AUTH"     # Auth tests
npx playwright test --grep "TC-NAV"      # Navigation tests

# Chạy headless (nhanh hơn, không hiện browser)
npx playwright test --headed=false

# Xem báo cáo HTML sau khi chạy
npx playwright show-report
```

## Lưu ý quan trọng

| Mục | Chi tiết |
|-----|----------|
| **BASE_URL** | Mặc định `https://localhost:7135` – đổi trong `playwright.config.ts` và đầu file spec nếu port khác |
| **Chạy tuần tự** | `workers: 1` để tránh conflict dữ liệu DB |
| **SSL localhost** | `ignoreHTTPSErrors: true` đã bật sẵn |
| **App phải chạy trước** | Start `dotnet run` trước khi chạy Playwright |
| **Headless** | Đổi `headless: true` trong config để chạy nhanh trên CI |

## Danh sách Test Cases

| Suite | ID | Mô tả |
|-------|----|-------|
| Auth | TC-AUTH-01 | Đăng nhập admin thành công |
| Auth | TC-AUTH-02 | Đăng nhập customer thành công |
| Auth | TC-AUTH-03 | Đăng nhập sai mật khẩu → hiện lỗi |
| Auth | TC-AUTH-04 | Đăng xuất thành công |
| Navigation | TC-NAV-01 | Vào Admin Panel từ navbar |
| Navigation | TC-NAV-02 | Sidebar → Manage Vehicles |
| Navigation | TC-NAV-03 | Sidebar → Manage Stations |
| Navigation | TC-NAV-04 | Sidebar → User Management |
| Navigation | TC-NAV-05 | Return to Home từ sidebar |
| Vehicle | TC-V01 | Tạo xe mới qua modal |
| Vehicle | TC-V02 | Validate form trống khi tạo xe |
| Vehicle | TC-V03 | Lọc xe theo trạng thái Available |
| Vehicle | TC-V04 | Tìm kiếm xe theo từ khóa |
| Station | TC-S01 | Tạo trạm mới thành công |
| Station | TC-S02 | Lọc trạm theo trạng thái Active |
| Station | TC-S03 | Tìm kiếm trạm theo tên |
| User Mgmt | TC-U01 | Mở modal tạo user mới |
| User Mgmt | TC-U02 | Tạo user mới thành công |
| User Mgmt | TC-U03 | Lọc user theo role Customer |
| Home | TC-HOME-01 | Trang chủ hiển thị danh sách trạm |
| Home | TC-HOME-02 | Tìm kiếm trạm trên trang chủ |
| Home | TC-HOME-03 | Nút thuê xe redirect Login khi chưa đăng nhập |
| Home | TC-HOME-04 | Customer mở modal thuê xe |
| Profile | TC-PROFILE-01 | Customer xem trang Profile |
| Profile | TC-PROFILE-02 | Cập nhật số điện thoại |
| Bug Report | TC-BUG-01 | Gửi bug report thành công |
| Bug Report | TC-BUG-02 | Gửi bug report thiếu tiêu đề → validation |
| Reports | TC-RPT-01 | Dashboard Admin load số liệu |
| Reports | TC-RPT-02 | Xem trang Reports |
| Reports | TC-RPT-03 | Xem Notifications |
| Rental | TC-RENTAL-01 | Customer xem lịch sử thuê xe |
