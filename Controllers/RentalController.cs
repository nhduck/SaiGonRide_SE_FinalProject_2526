using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using RentalVehicleService.Models;
using RentalVehicleService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VNPAY;
using VNPAY.Models;
using VNPAY.Models.Enums;
using VNPAY.Models.Exceptions;
using System.Text.RegularExpressions;

namespace RentalVehicleService.Controllers
{
    public class RentalController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly RentalService _rentalService;

        private readonly IVnpayClient _vnpayClient;

        private readonly IConfiguration _configuration;

        public RentalController(ApplicationDbContext context, RentalService rentalService, IVnpayClient vnpayClient, IConfiguration configuration)
        {
            _context = context;
            _rentalService = rentalService;
            _vnpayClient = vnpayClient;
            _configuration = configuration;
        }

        // Removed redundant constructor that caused CS8618

        // GET: Rental
        public async Task<IActionResult> Index()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.Rentals
                    .Include(r => r.EndStation)
                    .Include(r => r.StartStation)
                    .Include(r => r.Vehicle)
                    .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(r => r.UserId == userID);

            }

            return View(await query.OrderByDescending(r => r.StartTime).ToListAsync());
        }

        // GET: Rental/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.EndStation)
                .Include(r => r.StartStation)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(m => m.RentalId == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // 1. GET: Rental/Create (Màn hình xác nhận trung gian sau khi quét QR)
        [HttpGet]
        public async Task<IActionResult> Create(int vehicleId, int startStationId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            var station = await _context.Stations.FindAsync(startStationId);


            //tạm comment để hoàn thành(đây là kiểm tra trạng thái xe hợp lệ không)
            //if (vehicle == null || vehicle.State != VehicleState.Available)
            //{
            //    TempData["Error"] = "Xe không sẵn sàng hoặc không tồn tại.";
            //    return RedirectToAction("Index", "Home");
            //}

            ViewBag.VehicleId = vehicleId;
            ViewBag.StartStationId = startStationId;
            ViewBag.VehicleModel = vehicle?.VehicleModel ?? "Unknown";
            ViewBag.StationAddress = station?.Address ?? "Unknown Station";

            return View(); // Trả về màn hình xác nhận Create.cshtml
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreatePost(int vehicleId, int startStationId) // Tên hàm mới giúp hết lỗi CS0111
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);

            var rental = new Rental
            {
                VehicleId = vehicleId,
                StartStationId = startStationId,
                StartTime = DateTime.Now,
                Status = RentalStatus.Active,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                VehicleType = vehicle?.Type ?? VehicleType.Standard
            };

            try
            {
                if (vehicle != null) vehicle.State = VehicleState.Rented;
                var station = await _context.Stations.FindAsync(startStationId);
                if (station != null) station.CurrentCount -= 1;

                _context.Add(rental);
                await _context.SaveChangesAsync();

                // Chuyển hướng sang trang ActiveTrip kèm ID thực tế
                return RedirectToAction("ActiveTrip", "Rental", new { area = "", id = rental.RentalId });
            }
            catch (Exception ex)
            {
                return Content("Lỗi lưu DB: " + ex.Message);
            }
        }

        // GET: Rental/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
            {
                return NotFound();
            }
            ViewData["EndStationId"] = new SelectList(_context.Stations, "StationId", "Address", rental.EndStationId);
            ViewData["StartStationId"] = new SelectList(_context.Stations, "StationId", "Address", rental.StartStationId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "VehicleModel", rental.VehicleId);
            return View(rental);
        }

        // POST: Rental/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int endStationId)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            try
            {
                rental.EndTime = DateTime.Now;
                rental.EndStationId = endStationId;
                rental.Status = RentalStatus.Completed;

                rental.FinalFare = _rentalService.ProcessFinalBill(rental, endStationId);

                var vehicle = await _context.Vehicles.FindAsync((int)rental.VehicleId);
                if (vehicle != null) vehicle.State = VehicleState.Available;

                var station = await _context.Stations.FindAsync(endStationId);
                if (station != null) station.CurrentCount += 1;

                _context.Update(rental);
                await _context.SaveChangesAsync();

                return RedirectToAction("Payment", new { id = rental.RentalId });
            }
            catch (Exception)
            {
                return View(rental);
            }
        }
        [Authorize(Roles = "Admin")]
        // GET: Rental/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.EndStation)
                .Include(r => r.StartStation)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(m => m.RentalId == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: Rental/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
                if (vehicle != null && rental.Status == RentalStatus.Active)
                {
                    vehicle.State = VehicleState.Available;
                }

                _context.Rentals.Remove(rental);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RentalExists(int id)
        {
            return _context.Rentals.Any(e => e.RentalId == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableVehicles(int stationId, string type)
        {
            var vehicleType = type?.Equals("Electric", StringComparison.OrdinalIgnoreCase) == true
                              ? VehicleType.Electric
                              : VehicleType.Standard;

            var vehicles = await _context.Vehicles
                .Where(v => v.CurrentStationId == stationId
                         && v.Type == vehicleType
                         && v.State == VehicleState.Available)
                .ToListAsync();

            ViewBag.StationId = stationId;
            return PartialView("_VehicleListModal", vehicles);
        }
        public async Task<IActionResult> ActiveTrip(int id)
        {
            // Tìm chuyến đi dựa trên ID và nạp kèm thông tin xe/trạm
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.StartStation)
                .FirstOrDefaultAsync(m => m.RentalId == id);

            if (rental == null)
            {
                return NotFound();
            }

            ViewBag.Stations = await _context.Stations.ToListAsync();

            // Truyền dữ liệu chuyến đi sang trang ActiveTrip.cshtml
            return View(rental);
        }

        //Hàm payment trang thanh toán
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Payment(int id)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tìm chuyến đi cùng với thông tin Trạm trả và Xe
            var rental = await _context.Rentals
                .Include(r => r.EndStation)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(m => m.RentalId == id);

            if (rental == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(userID);

            // Tính tổng thời gian
            int totalMinutes = 0;
            if (rental.EndTime.HasValue)
            {
                totalMinutes = (int)(rental.EndTime.Value - rental.StartTime).TotalMinutes;
            }

            var model = new PaymentViewModel
            {
                CustomerName = user?.UserName ?? User.Identity?.Name ?? "Guest",
                PhoneNumber = user?.PhoneNumber ?? "No phone number available",
                Email = user?.Email ?? "No email yet",

                RentalId = rental.RentalId,
                EndStationName = rental.EndStation?.Address ?? "There is no pay station yet",
                EndStationAddress = rental.EndStation?.Address ?? "No address available",
                VehicleModel = rental.Vehicle?.VehicleModel ?? "Not determined",

                // VehicleBattery = rental.Vehicle?.BatteryLevel, 

                StartTime = rental.StartTime,
                TotalMinutes = totalMinutes,
                FinalFare = rental.FinalFare,
                DiscountAmount = rental.DiscountAmount
            };

            return View(model);
        }

        // THÊM MỚI: Xử lý nút bấm "Xác nhận & Thanh toán"
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ProcessPayment(int rentalId, string paymentMethod, string couponCode)
        {
            var rental = await _context.Rentals.FindAsync(rentalId);
            if (rental == null) return NotFound();

            if (paymentMethod == "VNPay")
            {
                decimal finalAmount = rental.FinalFare;

                if (!string.IsNullOrEmpty(couponCode) && couponCode.ToUpper() == "SAIGONGREEN20")
                {
                    finalAmount -= 10000;
                    if (finalAmount < 0) finalAmount = 0;
                }

                var request = new VnpayPaymentRequest
                {
                    Money = (double)finalAmount,
                    Description = $"Thanh toan chuyen di {rentalId} tai SaigonRide",
                    BankCode = BankCode.ANY
                };

                var paymentUrlInfo = _vnpayClient.CreatePaymentUrl(request);
                return Redirect(paymentUrlInfo.Url);
            }

            TempData["Info"] = "This payment method is currently under development..";
            return RedirectToAction("Payment", new { id = rentalId });
        }

        //Xử lý kết quả trả về từ VNPay
       [HttpGet]
        [Authorize]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                var paymentResult = _vnpayClient.GetPaymentResult(Request);
                var match = Regex.Match(paymentResult.Description, @"\d+");
                if (!match.Success)
                {
                    // Nếu không tìm thấy số nào trong chuỗi, báo lỗi hoặc quay về trang chủ
                    TempData["Error"] = "Không tìm thấy mã chuyến đi trong giao dịch.";
                    return RedirectToAction("Index", "Home");
                }


                int rentalId = int.Parse(match.Value);

                // Thanh toán thành công
                var rental = await _context.Rentals.FindAsync(rentalId);
                if (rental != null)
                {
                    rental.Status = RentalStatus.Completed;
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Payment successful! Thank you for using saigonRide.";
                return View("Success");
            }
            catch (VnpayException ex) // Thanh toán thất bại / sai chữ ký
            {
                // Parse rentalId từ query string trực tiếp để redirect về Payment
                var txnRef = Request.Query["vnp_TxnRef"].ToString();
                int rentalId = string.IsNullOrEmpty(txnRef) ? 0 : int.Parse(txnRef.Split('_')[0]);

                TempData["Error"] = "Payment failed " + ex.Message;
                return rentalId > 0
                    ? RedirectToAction("Payment", new { id = rentalId })
                    : RedirectToAction("Index");
            }
        }
    }
}
