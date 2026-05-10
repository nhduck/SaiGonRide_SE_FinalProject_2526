# SaigonRide - Final Project Demo Script (Bilingual)

**Total Duration:** 15-18 Minutes
**Authors:** Thanh, Duc, Dat
**Structure:** Introduction -> Features -> Rental Flow -> Payment -> Admin -> Conclusion

---

## 1. Introduction | Giới thiệu (2 mins)

### English:
"Good morning Professor. I am [Your Name], and on behalf of our team, I am honored to present our final project: **SaigonRide**. Our platform is a smart bike-sharing solution tailored for Ho Chi Minh City, aiming to provide a green, convenient, and tech-driven mobility option for residents and tourists alike."

### Vietnamese:
"Chào thầy. Em là [Tên của bạn], thay mặt cho nhóm, em xin phép được trình bày đồ án cuối kỳ của chúng em với tên gọi: **SaigonRide**. Đây là nền tảng cho thuê xe đạp thông minh được thiết kế riêng cho TP. Hồ Chí Minh, với mục tiêu cung cấp giải pháp di chuyển xanh, tiện lợi và hiện đại cho cả người dân và khách du lịch."

---

## 2. Landing Page & i18n | Trang chủ & Đa ngôn ngữ (2 mins)

### English:
"As you can see, our landing page features a premium dark mode design with glassmorphism effects. One of our core features is the **Real-time Bilingual Support**. Users can toggle between English and Vietnamese instantly without reloading the page. All platform statistics, such as total stations and vehicles, are fetched dynamically from our PostgreSQL database."

### Vietnamese:
"Như thầy có thể thấy, trang chủ được thiết kế theo phong cách Dark Mode cao cấp với hiệu ứng Glassmorphism. Một trong những tính năng cốt lõi là **Hỗ trợ đa ngôn ngữ thời gian thực**. Người dùng có thể chuyển đổi giữa tiếng Anh và tiếng Việt ngay lập tức mà không cần tải lại trang. Tất cả số liệu thống kê như tổng số trạm và xe đều được lấy động từ cơ sở dữ liệu PostgreSQL."

---

## 3. Account & Security | Tài khoản & Bảo mật (3 mins)

### English:
"Security is our priority. We implemented a **6-digit Email OTP Verification** system. During registration, users must verify their identity. We support two user types: **Local Users** (via National ID/CCCD) and **Tourists** (via Passport). If a user forgets their password, they can recover it through a secure OTP flow."

### Vietnamese:
"Bảo mật là ưu tiên hàng đầu của chúng em. Nhóm đã triển khai hệ thống **Xác thực mã OTP 6 số qua Email**. Khi đăng ký, người dùng phải xác minh danh tính. Chúng em hỗ trợ hai loại đối tượng: **Người dân địa phương** (qua số CCCD) và **Khách du lịch** (qua số Hộ chiếu). Nếu quên mật khẩu, người dùng có thể khôi phục thông qua quy trình OTP an toàn."

---

## 4. Rental Workflow | Quy trình thuê xe (4 mins)

### English:
"Now, let's look at the core rental flow. Users can find stations on our interactive map. Each station displays live inventory status: **Normal**, **Low stock**, or **Almost Full**. To rent, users simply scan the vehicle's **QR Code**. The system then calculates the trip cost per minute. Notice the **Rebalancing Incentive**: ending a trip at a low-stock station automatically applies a **15% discount** to encourage fleet distribution."

### Vietnamese:
"Tiếp theo là quy trình thuê xe cốt lõi. Người dùng có thể tìm trạm trên bản đồ tương tác. Mỗi trạm hiển thị trạng thái tồn kho thực tế: **Bình thường**, **Sắp hết xe**, hoặc **Sắp đầy chỗ**. Để thuê, người dùng chỉ cần quét **Mã QR** của xe. Hệ thống sẽ tính phí theo phút. Đặc biệt, chúng em có cơ chế **Khuyến khích điều phối**: kết thúc chuyến đi tại trạm đang thiếu xe sẽ được **giảm giá 15%** tự động."

---

## 5. Payment Gateways | Cổng thanh toán (3 mins)

### English:
"SaigonRide uses the **Strategy Pattern** for payments. Once a trip is ended, users can pay via **VNPay** (for local users) or **PayPal** (for international tourists). We've also included a **Coupon System** where users can enter codes like 'SAIGONGREEN' to get discounts. All transactions are processed through secure sandbox environments."

### Vietnamese:
"SaigonRide sử dụng **Strategy Pattern** cho hệ thống thanh toán. Sau khi kết thúc chuyến đi, người dùng có thể thanh toán qua **VNPay** (cho người dân nội địa) hoặc **PayPal** (cho khách quốc tế). Chúng em cũng tích hợp **Hệ thống mã giảm giá**, nơi người dùng có thể nhập các mã như 'SAIGONGREEN'. Tất cả giao dịch đều được xử lý qua môi trường Sandbox an toàn."

---

## 6. Admin Dashboard | Bảng điều khiển Admin (3 mins)

### English:
"Lastly, the **Admin Dashboard** provides powerful management tools. Admins can monitor revenue analytics for the last 7 days, manage station inventory, and track vehicle battery levels. If a vehicle's battery is below 20%, it is automatically flagged for maintenance. Admins can also export revenue reports to CSV with full UTF-8 support."

### Vietnamese:
"Cuối cùng, **Bảng điều khiển Admin** cung cấp các công cụ quản lý mạnh mẽ. Quản trị viên có thể theo dõi biểu đồ doanh thu 7 ngày qua, quản lý tồn kho tại các trạm và theo dõi mức pin của xe. Nếu pin dưới 20%, xe sẽ tự động được đánh dấu để bảo trì. Admin cũng có thể xuất báo cáo doanh thu ra file CSV hỗ trợ đầy đủ tiếng Việt."

---

## 7. Conclusion | Kết thúc (1 min)

### English:
"That concludes our demonstration. SaigonRide is more than just a coding project; it's a vision for a greener Ho Chi Minh City. Thank you Professor for your guidance, and we are now ready for any questions."

### Vietnamese:
"Đó là toàn bộ phần trình bày của nhóm em. SaigonRide không chỉ là một đồ án lập trình, mà còn là tầm nhìn về một TP. Hồ Chí Minh xanh hơn. Cám ơn thầy đã lắng nghe và hướng dẫn, chúng em sẵn sàng trả lời các câu hỏi của thầy."

---

## Presentation Tips | Mẹo thuyết trình:
1.  **Prep:** Have the database seeded and at least one active rental ready to show.
2.  **Payment:** Use the VNPay Test Card (9704198888888888888) for the demo.
3.  **Language:** Show the toggle at least twice during the video.
4.  **Admin:** Highlight the "Battery Warning" icons to show attention to detail.
