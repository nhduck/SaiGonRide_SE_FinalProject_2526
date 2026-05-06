-- PayPal & Vehicle Sync - Diagnostic SQL Queries
-- Run these queries to verify all fixes are working correctly

-- ============================================
-- 1. CHECK COMPLETED RENTALS WITH PAYMENT INFO
-- ============================================
SELECT TOP 20
    r.RentalId,
    r.UserId,
    v.VehicleModel,
    s_start.Address as 'Start Station',
    s_end.Address as 'End Station',
    r.Status,
    r.PaymentMethod,
    r.PaymentTransactionId,
    r.PaymentCompletedTime,
    r.FinalFare,
    v.State as 'Vehicle State'
FROM Rentals r
LEFT JOIN Vehicles v ON r.VehicleId = v.VehicleId
LEFT JOIN Stations s_start ON r.StartStationId = s_start.StationId
LEFT JOIN Stations s_end ON r.EndStationId = s_end.StationId
WHERE r.Status = 'Completed'
ORDER BY r.PaymentCompletedTime DESC;

-- ============================================
-- 2. VERIFY VEHICLE STATES ARE CORRECT
-- ============================================
SELECT 
    v.VehicleId,
    v.VehicleModel,
    v.State,
    v.CurrentStationId,
    s.Name as 'Station Name',
    s.CurrentCount,
    s.TotalCapacity,
    CASE 
        WHEN v.State = 'Available' AND s.CurrentCount > 0 THEN 'OK'
        WHEN v.State = 'Available' AND s.CurrentCount = s.TotalCapacity THEN 'FULL'
        WHEN v.State = 'Rented' THEN 'IN USE'
        WHEN v.State = 'Maintenance' THEN 'MAINTENANCE'
        WHEN v.State = 'Charging' THEN 'CHARGING'
        ELSE 'CHECK'
    END as 'Status Check'
FROM Vehicles v
LEFT JOIN Stations s ON v.CurrentStationId = s.StationId
ORDER BY v.VehicleId;

-- ============================================
-- 3. CHECK STATION COUNTS
-- ============================================
SELECT 
    StationId,
    Name,
    Address,
    CurrentCount,
    TotalCapacity,
    (TotalCapacity - CurrentCount) as 'In Use',
    ROUND(CAST(CurrentCount AS FLOAT) / TotalCapacity * 100, 2) as 'Availability %'
FROM Stations
ORDER BY Name;

-- ============================================
-- 4. FIND RENTALS NOT YET PAID
-- ============================================
SELECT 
    r.RentalId,
    r.UserId,
    v.VehicleModel,
    r.StartTime,
    r.EndTime,
    r.Status,
    r.FinalFare,
    r.PaymentMethod,
    DATEDIFF(HOUR, r.EndTime, GETDATE()) as 'Hours Since Returned'
FROM Rentals r
LEFT JOIN Vehicles v ON r.VehicleId = v.VehicleId
WHERE r.Status = 'PendingPayment'
AND r.EndTime IS NOT NULL
ORDER BY r.EndTime DESC;

-- ============================================
-- 5. VERIFY PAYPAL PAYMENTS ARE SAVED
-- ============================================
SELECT 
    RentalId,
    PaymentMethod,
    PaymentTransactionId,
    PaymentCompletedTime,
    FinalFare
FROM Rentals
WHERE PaymentMethod = 'PayPal'
AND PaymentTransactionId IS NOT NULL
ORDER BY PaymentCompletedTime DESC;

-- ============================================
-- 6. VERIFY VNPAY PAYMENTS ARE SAVED
-- ============================================
SELECT 
    RentalId,
    PaymentMethod,
    PaymentTransactionId,
    PaymentCompletedTime,
    FinalFare
FROM Rentals
WHERE PaymentMethod = 'VNPay'
AND PaymentTransactionId IS NOT NULL
ORDER BY PaymentCompletedTime DESC;

-- ============================================
-- 7. FIND ANY COMPLETED RENTALS WITHOUT PAYMENT INFO
-- ============================================
SELECT 
    RentalId,
    UserId,
    Status,
    FinalFare,
    PaymentMethod,
    PaymentTransactionId,
    'MISSING PAYMENT INFO' as 'WARNING'
FROM Rentals
WHERE Status = 'Completed'
AND (PaymentMethod IS NULL OR PaymentTransactionId IS NULL)
ORDER BY RentalId DESC;

-- ============================================
-- 8. PAYMENT SUMMARY BY METHOD
-- ============================================
SELECT 
    COALESCE(PaymentMethod, 'No Payment') as 'Payment Method',
    COUNT(*) as 'Total Rentals',
    SUM(FinalFare) as 'Total Amount (VND)',
    COUNT(CASE WHEN Status = 'Completed' THEN 1 END) as 'Completed',
    COUNT(CASE WHEN Status = 'PendingPayment' THEN 1 END) as 'Pending',
    MIN(PaymentCompletedTime) as 'First Payment',
    MAX(PaymentCompletedTime) as 'Last Payment'
FROM Rentals
GROUP BY PaymentMethod
ORDER BY COUNT(*) DESC;

-- ============================================
-- 9. USER PAYMENT HISTORY
-- ============================================
-- Replace 'USER_ID' with actual user ID
SELECT 
    r.RentalId,
    r.StartTime,
    r.EndTime,
    r.FinalFare,
    r.PaymentMethod,
    r.PaymentTransactionId,
    r.PaymentCompletedTime,
    s_start.Address as 'From',
    s_end.Address as 'To'
FROM Rentals r
LEFT JOIN Stations s_start ON r.StartStationId = s_start.StationId
LEFT JOIN Stations s_end ON r.EndStationId = s_end.StationId
WHERE r.UserId = 'USER_ID'
AND r.Status = 'Completed'
ORDER BY r.PaymentCompletedTime DESC;

-- ============================================
-- 10. DATA INTEGRITY CHECK
-- ============================================
SELECT 
    'Total Rentals' as 'Metric',
    COUNT(*) as 'Count'
FROM Rentals

UNION ALL

SELECT 
    'Completed Rentals',
    COUNT(*)
FROM Rentals
WHERE Status = 'Completed'

UNION ALL

SELECT 
    'Completed with Payment Info',
    COUNT(*)
FROM Rentals
WHERE Status = 'Completed'
AND PaymentMethod IS NOT NULL
AND PaymentTransactionId IS NOT NULL

UNION ALL

SELECT 
    'Pending Payment Rentals',
    COUNT(*)
FROM Rentals
WHERE Status = 'PendingPayment'

UNION ALL

SELECT 
    'Active Rentals',
    COUNT(*)
FROM Rentals
WHERE Status = 'Active'

UNION ALL

SELECT 
    'Available Vehicles',
    COUNT(*)
FROM Vehicles
WHERE State = 'Available'

UNION ALL

SELECT 
    'Rented Vehicles',
    COUNT(*)
FROM Vehicles
WHERE State = 'Rented'

UNION ALL

SELECT 
    'Total Vehicles',
    COUNT(*)
FROM Vehicles;

-- ============================================
-- 11. DETECT SYNC ISSUES
-- ============================================
SELECT 
    r.RentalId,
    v.VehicleId,
    v.VehicleModel,
    v.State as 'Vehicle State',
    r.Status as 'Rental Status',
    CASE 
        WHEN r.Status = 'Active' AND v.State != 'Rented' THEN 'ERROR: Active rental but vehicle not rented'
        WHEN r.Status = 'Completed' AND v.State != 'Available' THEN 'ERROR: Completed rental but vehicle not available'
        WHEN r.Status = 'PendingPayment' AND v.State != 'Available' THEN 'ERROR: Payment pending but vehicle not available'
        ELSE 'OK'
    END as 'Sync Status'
FROM Rentals r
LEFT JOIN Vehicles v ON r.VehicleId = v.VehicleId
WHERE (r.Status = 'Completed' AND v.State != 'Available')
   OR (r.Status = 'Active' AND v.State != 'Rented')
ORDER BY r.RentalId DESC;

-- ============================================
-- 12. RECENT ACTIVITY LOG
-- ============================================
SELECT TOP 50
    RentalId,
    Status,
    StartTime,
    EndTime,
    PaymentCompletedTime,
    PaymentMethod,
    DATEDIFF(MINUTE, COALESCE(PaymentCompletedTime, EndTime, StartTime), GETDATE()) as 'Minutes Ago'
FROM Rentals
ORDER BY COALESCE(PaymentCompletedTime, EndTime, StartTime) DESC;
