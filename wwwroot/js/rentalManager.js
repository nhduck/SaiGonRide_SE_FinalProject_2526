function startRentalTimer() {
    const costElement = document.getElementById('cost');
    const timerElement = document.getElementById('timer');

    // Lấy giá/phút
    let pricePerMinute = 1500;
    if (costElement) {
        const priceAttr = costElement.getAttribute('data-price-per-minute');
        if (priceAttr && !isNaN(priceAttr)) {
            pricePerMinute = parseFloat(priceAttr);
        }
    }

    // Lấy thời điểm bắt đầu từ server (ISO string)
    const startTimeAttr = timerElement ? timerElement.getAttribute('data-start-time') : null;
    const startTime = startTimeAttr ? new Date(startTimeAttr) : new Date();

    setInterval(() => {
        // Tính số giây đã trôi qua từ lúc bắt đầu thực sự
        const totalSeconds = Math.floor((new Date() - startTime) / 1000);

        // 1. Cập nhật Đồng hồ
        const hrs = Math.floor(totalSeconds / 3600).toString().padStart(2, '0');
        const mins = Math.floor((totalSeconds % 3600) / 60).toString().padStart(2, '0');
        const secs = (totalSeconds % 60).toString().padStart(2, '0');

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