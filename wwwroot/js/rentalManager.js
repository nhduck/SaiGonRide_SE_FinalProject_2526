/**
 * SaigonRide Rental Manager 
 * International Version
 */

let totalSeconds = 0;
const pricePerMinute = 2000; // 2,000 VND per minute

function startRentalTimer() {
    setInterval(() => {
        totalSeconds++;

        // 1. Update Timer Display (HH : MM : SS)
        const hrs = Math.floor(totalSeconds / 3600).toString().padStart(2, '0');
        const mins = Math.floor((totalSeconds % 3600) / 60).toString().padStart(2, '0');
        const secs = (totalSeconds % 60).toString().padStart(2, '0');

        const timerElement = document.getElementById('timer');
        if (timerElement) {
            timerElement.innerText = `${hrs} : ${mins} : ${secs}`;
        }

        // 2. Update Estimated Cost
        const costElement = document.getElementById('cost');
        if (costElement) {
            const currentCost = (totalSeconds / 60) * pricePerMinute;
            // Use 'en-US' for international number format but keep VND
            costElement.innerText = currentCost.toLocaleString('en-US', { maximumFractionDigits: 0 }) + " VND";
        }

        // 3. Update Distance (Simulating 15km/h speed)
        const distElement = document.getElementById('distance');
        if (distElement) {
            const distance = (totalSeconds * 0.0041).toFixed(2);
            distElement.innerText = distance + " km";
        }
    }, 1000);
}

// Initializing the timer on page load
document.addEventListener('DOMContentLoaded', startRentalTimer);