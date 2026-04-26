using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentalVehicleService.Models;
using RentalVehicleService.Models.ViewModels;

namespace RentalVehicleService.Controllers
{
    public class UserManagementsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementsController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ════════════════════════════════════════════════════════
        // HELPER — chuyển ApplicationUser → UserManagement ViewModel
        // ════════════════════════════════════════════════════════

        private async Task<UserManagement> MapToViewModel(ApplicationUser u)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var isLocked = await _userManager.IsLockedOutAsync(u);

            return new UserManagement
            {
                Id = u.Id,
                UserName = u.UserName ?? "",
                Email = u.Email ?? "",
                PhoneNumber = u.PhoneNumber ?? "",
                FullName = u.FullName,
                UserType = u.UserType,
                CCCD = u.CCCD,
                PassportNumber = u.PassportNumber,
                Nationality = u.Nationality,
                IsLockedOut = isLocked,
                Roles = roles
            };
        }

        private async Task<List<UserManagement>> GetAllViewModels()
        {
            var appUsers = _userManager.Users.ToList();
            var result = new List<UserManagement>();

            foreach (var u in appUsers)
                result.Add(await MapToViewModel(u));

            return result;
        }


        // ════════════════════════════════════════════════════════
        // INDEX
        // ════════════════════════════════════════════════════════

        public async Task<IActionResult> Index()
        {
            var users = await GetAllViewModels();

            ViewBag.TotalUsers = users.Count;
            ViewBag.ActiveUsers = users.Count(u => !u.IsLockedOut);
            ViewBag.LockedUsers = users.Count(u => u.IsLockedOut);
            ViewBag.Admins = users.Count(u => u.Roles.Contains("Admin"));
            ViewBag.Customers = users.Count(u => u.Roles.Contains("Customer"));
            ViewBag.Staff = users.Count(u => u.Roles.Contains("Staff"));

            return PartialView("~/Views/AdminDashboard/Pages/UserManagements/Index.cshtml",
                               users.AsEnumerable());
        }


        // ════════════════════════════════════════════════════════
        // SEARCH (gọi từ AdminDashboard/SearchUsers hoặc trực tiếp)
        // ════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string searchTerm, List<string> filters)
        {
            var users = await GetAllViewModels();

            // Tìm kiếm theo text
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                users = users.Where(u =>
                    u.FullName.ToLower().Contains(term) ||
                    u.UserName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    u.Id.ToLower().Contains(term)
                ).ToList();
            }

            // Lọc theo filter button
            if (filters != null && filters.Any())
            {
                users = users.Where(u =>
                    (filters.Contains("Active") && !u.IsLockedOut) ||
                    (filters.Contains("Locked") && u.IsLockedOut) ||
                    (filters.Contains("Admin") && u.Roles.Contains("Admin")) ||
                    (filters.Contains("Customer") && u.Roles.Contains("Customer")) ||
                    (filters.Contains("Staff") && u.Roles.Contains("Staff"))
                ).ToList();
            }

            return PartialView(
                "~/Views/AdminDashboard/Pages/UserManagements/_UserTablePartial.cshtml",
                users.AsEnumerable());
        }


        // ════════════════════════════════════════════════════════
        // GET AMOUNT INFO — cập nhật stats card sau mỗi thao tác
        // ════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetAmountInfo()
        {
            var users = await GetAllViewModels();

            return Json(new
            {
                totalUsers = users.Count,
                activeUsers = users.Count(u => !u.IsLockedOut),
                lockedUsers = users.Count(u => u.IsLockedOut),
                admins = users.Count(u => u.Roles.Contains("Admin")),
                customers = users.Count(u => u.Roles.Contains("Customer")),
                staff = users.Count(u => u.Roles.Contains("Staff"))
            });
        }


        // ════════════════════════════════════════════════════════
        // DETAILS — trả HTML partial load vào modal body
        // ════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("User ID is required.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("User not found.");

            var vm = await MapToViewModel(user);

            return PartialView(
                "~/Views/AdminDashboard/Pages/UserManagements/_UserDetailsBody.cshtml", vm);
        }


        // ════════════════════════════════════════════════════════
        // CREATE — GET trả JSON (không dùng), POST xử lý form
        // ════════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string FullName,
            string UserName,
            string Email,
            string PhoneNumber,
            string Password,
            string UserType,
            string? CCCD,
            string? PassportNumber,
            string? Nationality,
            List<string>? Roles)
        {
            // Kiểm tra username / email trùng
            if (await _userManager.FindByNameAsync(UserName) != null)
                return BadRequest(new { errors = new[] { "Username already exists." } });

            if (await _userManager.FindByEmailAsync(Email) != null)
                return BadRequest(new { errors = new[] { "Email already exists." } });

            var newUser = new ApplicationUser
            {
                UserName = UserName,
                Email = Email,
                PhoneNumber = PhoneNumber,
                FullName = FullName,
                UserType = UserType ?? "Local",
                CCCD = CCCD,
                PassportNumber = PassportNumber,
                Nationality = Nationality,
                EmailConfirmed = true   // bỏ qua xác thực email cho admin tạo
            };

            var result = await _userManager.CreateAsync(newUser, Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { errors });
            }

            // Gán roles nếu có
            if (Roles != null && Roles.Any())
            {
                foreach (var role in Roles)
                {
                    // Tạo role nếu chưa tồn tại
                    if (!await _roleManager.RoleExistsAsync(role))
                        await _roleManager.CreateAsync(new IdentityRole(role));

                    await _userManager.AddToRoleAsync(newUser, role);
                }
            }

            return Ok();
        }


        // ════════════════════════════════════════════════════════
        // EDIT GET — trả JSON cho JS điền vào form
        // ════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("User ID is required.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            return Json(new
            {
                id = user.Id,
                fullName = user.FullName,
                userName = user.UserName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                userType = user.UserType,
                cccd = user.CCCD,
                passportNumber = user.PassportNumber,
                nationality = user.Nationality,
                isLockedOut = await _userManager.IsLockedOutAsync(user),
                roles
            });
        }


        // ════════════════════════════════════════════════════════
        // EDIT POST — lưu thay đổi
        // ════════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string Id,
            string FullName,
            string UserName,
            string Email,
            string PhoneNumber,
            string UserType,
            string? CCCD,
            string? PassportNumber,
            string? Nationality,
            bool IsLockedOut,
            List<string>? Roles,
            string? NewPassword,
            string? ConfirmPassword)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
                return NotFound("User not found.");

            // Cập nhật thông tin cơ bản
            user.FullName = FullName;
            user.UserName = UserName;
            user.Email = Email;
            user.PhoneNumber = PhoneNumber;
            user.UserType = UserType ?? "Local";
            user.CCCD = CCCD;
            user.PassportNumber = PassportNumber;
            user.Nationality = Nationality;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { errors });
            }

            // Lock / Unlock
            if (IsLockedOut)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            else
                await _userManager.SetLockoutEndDateAsync(user, null);

            // Cập nhật roles — xóa hết rồi gán lại
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (Roles != null && Roles.Any())
            {
                foreach (var role in Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                        await _roleManager.CreateAsync(new IdentityRole(role));

                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            // Đổi mật khẩu nếu có nhập
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword != ConfirmPassword)
                    return BadRequest(new { errors = new[] { "Passwords do not match." } });

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, NewPassword);

                if (!passwordResult.Succeeded)
                {
                    var errors = passwordResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(new { errors });
                }
            }

            return Ok();
        }


        // ════════════════════════════════════════════════════════
        // DELETE
        // ════════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("User ID is required.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("User not found.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { errors });
            }

            return Ok();
        }


        // ════════════════════════════════════════════════════════
        // TOGGLE LOCK — quick action từ bảng
        // ════════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("User ID is required.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("User not found.");

            var isLocked = await _userManager.IsLockedOutAsync(user);

            if (isLocked)
                await _userManager.SetLockoutEndDateAsync(user, null);              // mở khóa
            else
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue); // khóa

            return Ok();
        }
    }
}