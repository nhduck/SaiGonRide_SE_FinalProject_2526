let totalSeconds = 0;

function startRentalTimer() {
    const costElement = document.getElementById('cost');

    // Đặt giá mặc định phòng hờ lỗi
    let pricePerMinute = 1000;

    // Đọc trực tiếp giá tiền từ C# truyền xuống qua HTML
    if (costElement) {
        const priceAttr = costElement.getAttribute('data-price-per-minute');

        // Kiểm tra xem giá trị có tồn tại và có phải là số hợp lệ không
        if (priceAttr && !isNaN(priceAttr)) {
            pricePerMinute = parseFloat(priceAttr);
        }
        console.log("Đơn giá hiện tại lấy từ HTML: " + pricePerMinute);
    }

    setInterval(() => {
        totalSeconds++;

        // 1. Cập nhật Đồng hồ
        const hrs = Math.floor(totalSeconds / 3600).toString().padStart(2, '0');
        const mins = Math.floor((totalSeconds % 3600) / 60).toString().padStart(2, '0');
        const secs = (totalSeconds % 60).toString().padStart(2, '0');

        const timerElement = document.getElementById('timer');
        if (timerElement) {
            timerElement.innerText = `${hrs} : ${mins} : ${secs}`;
        }

        // 2. Cập nhật Giá tiền
        if (costElement) {
            const currentCost = (totalSeconds / 60) * pricePerMinute;
            costElement.innerText = currentCost.toLocaleString('en-US', { maximumFractionDigits: 0 }) + " VND";
        }

        // 3. Cập nhật Quãng đường
        const distElement = document.getElementById('distance');
        if (distElement) {
            const distance = (totalSeconds * 0.0041).toFixed(2);
            distElement.innerText = distance + " km";
        }
    }, 1000);
}

document.addEventListener('DOMContentLoaded', startRentalTimer);