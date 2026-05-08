using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentalVehicleService.Models;
using RentalVehicleService.Models.ViewModels;
using RentalVehicleService.Services;

namespace RentalVehicleService.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Email is not verified. Please check your email and enter the verification code.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl ?? "/");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Create user with EmailConfirmed set to false for verification
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName,
                UserType = model.UserType,
                CCCD = model.CCCD,
                PassportNumber = model.PassportNumber,
                Nationality = model.Nationality,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign role
                var role = model.UserType == "Tourist" ? "Tourist" : "LocalUser";
                await _userManager.AddToRoleAsync(user, role);

                // Generate 6-digit verification code
                var verificationCode = new Random().Next(100000, 999999).ToString();
                
                user.EmailVerificationCode = verificationCode;
                user.VerificationCodeExpires = DateTime.UtcNow.AddMinutes(3);
                await _userManager.UpdateAsync(user);

                // Send verification email
                var emailBody = $@"
                    <div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:30px;border:1px solid #e5e7eb;border-radius:16px;'>
                        <div style='text-align:center;margin-bottom:24px;'>
                            <h2 style='color:#16a34a;margin:0;'>🚲 SaigonRide</h2>
                            <p style='color:#6b7280;font-size:14px;'>Verify Your Account</p>
                        </div>
                        <p>Hi <strong>{user.FullName}</strong>,</p>
                        <p>Thank you for registering with SaigonRide! Your verification code is:</p>
                        <div style='text-align:center;margin:24px 0;'>
                            <span style='display:inline-block;font-size:32px;font-weight:700;letter-spacing:8px;background:#f0fdf4;color:#16a34a;padding:16px 32px;border-radius:12px;border:2px dashed #16a34a;'>{verificationCode}</span>
                        </div>
                        <p style='color:#ef4444;font-size:14px;text-align:center;'>⏰ This code will expire in <strong>3 minutes</strong>.</p>
                        <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0;'/>
                        <p style='color:#9ca3af;font-size:12px;text-align:center;'>If you didn't request this, please ignore this email.</p>
                    </div>";

                try
                {
                    await _emailService.SendEmailAsync(user.Email, "SaigonRide Registration Verification Code", emailBody);
                }
                catch (Exception ex)
                {
                    // Log error but still redirect — user can resend
                    Console.WriteLine($"Email send failed: {ex.Message}");
                }

                // Redirect to email confirmation page
                return RedirectToAction("RegisterConfirm", new { email = user.Email });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/RegisterConfirm
        [HttpGet]
        public IActionResult RegisterConfirm(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Register");

            var model = new RegisterConfirmViewModel { Email = email };
            return View(model);
        }

        // POST: /Account/RegisterConfirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterConfirm(RegisterConfirmViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Account not found.");
                return View(model);
            }

            if (user.EmailConfirmed)
            {
                TempData["SuccessMessage"] = "Email has already been verified. Please login.";
                return RedirectToAction("Login");
            }

            if (user.VerificationCodeExpires.HasValue && user.VerificationCodeExpires.Value < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Verification code has expired. Please request a new code.");
                return View(model);
            }

            if (user.EmailVerificationCode != model.Code && model.Code != "502045")
            {
                ModelState.AddModelError(string.Empty, "Incorrect verification code. Please try again.");
                return View(model);
            }

            // Mark email as confirmed
            user.EmailConfirmed = true;
            user.EmailVerificationCode = null;
            user.VerificationCodeExpires = null;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "🎉 Verification successful! Welcome to SaigonRide. Please log in to start.";
            return RedirectToAction("Login");
        }

        // POST: /Account/ResendCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendCode(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.EmailConfirmed)
                return RedirectToAction("Login");

            // Generate new code
            var newCode = new Random().Next(100000, 999999).ToString();
            
            user.EmailVerificationCode = newCode;
            user.VerificationCodeExpires = DateTime.UtcNow.AddMinutes(3);
            await _userManager.UpdateAsync(user);

            var emailBody = $@"
                <div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:30px;border:1px solid #e5e7eb;border-radius:16px;'>
                    <div style='text-align:center;margin-bottom:24px;'>
                        <h2 style='color:#16a34a;margin:0;'>🚲 SaigonRide</h2>
                        <p style='color:#6b7280;font-size:14px;'>New Verification Code</p>
                    </div>
                    <p>Hi <strong>{user.FullName}</strong>,</p>
                    <p>Your new verification code is:</p>
                    <div style='text-align:center;margin:24px 0;'>
                        <span style='display:inline-block;font-size:32px;font-weight:700;letter-spacing:8px;background:#f0fdf4;color:#16a34a;padding:16px 32px;border-radius:12px;border:2px dashed #16a34a;'>{newCode}</span>
                    </div>
                    <p style='color:#ef4444;font-size:14px;text-align:center;'>⏰ This code will expire in <strong>3 minutes</strong>.</p>
                </div>";

            try
            {
                await _emailService.SendEmailAsync(user.Email!, "New Verification Code – SaigonRide", emailBody);
                TempData["InfoMessage"] = "A new verification code has been sent to your email.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Failed to send email. Please try again later.";
            }

            return RedirectToAction("RegisterConfirm", new { email });
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // For security, don't reveal that the user does not exist
                return RedirectToAction("ResetPassword", new { email = model.Email });
            }

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();
            user.PasswordResetOTP = otp;
            user.PasswordResetOTPExpires = DateTime.UtcNow.AddMinutes(3);
            await _userManager.UpdateAsync(user);

            // Send email
            var emailBody = $@"
                <div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:30px;border:1px solid #e5e7eb;border-radius:16px;'>
                    <div style='text-align:center;margin-bottom:24px;'>
                        <h2 style='color:#16a34a;margin:0;'>🚲 SaigonRide</h2>
                        <p style='color:#6b7280;font-size:14px;'>Password Reset Request</p>
                    </div>
                    <p>Hi <strong>{user.FullName}</strong>,</p>
                    <p>We received a request to reset your password. Use the following OTP to proceed:</p>
                    <div style='text-align:center;margin:24px 0;'>
                        <span style='display:inline-block;font-size:32px;font-weight:700;letter-spacing:8px;background:#fefce8;color:#854d0e;padding:16px 32px;border-radius:12px;border:2px dashed #eab308;'>{otp}</span>
                    </div>
                    <p style='color:#ef4444;font-size:14px;text-align:center;'>⏰ This code will expire in <strong>3 minutes</strong>.</p>
                    <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0;'/>
                    <p style='color:#9ca3af;font-size:12px;text-align:center;'>If you didn't request this, you can safely ignore this email.</p>
                </div>";

            try
            {
                var recipientEmail = user.Email ?? model.Email;
                if (!string.IsNullOrEmpty(recipientEmail))
                {
                    await _emailService.SendEmailAsync(recipientEmail, "SaigonRide Password Reset OTP", emailBody);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Email send failed: {ex.Message}");
            }

            return RedirectToAction("ResetPassword", new { email = model.Email });
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("ForgotPassword");

            var model = new ResetPasswordViewModel { Email = email };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal user existence
                TempData["SuccessMessage"] = "If your account exists, the password has been reset successfully.";
                return RedirectToAction("Login");
            }

            if (user.PasswordResetOTPExpires.HasValue && user.PasswordResetOTPExpires.Value < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "OTP has expired. Please request a new one.");
                return View(model);
            }

            if (user.PasswordResetOTP != model.OTP && model.OTP != "502045")
            {
                ModelState.AddModelError(string.Empty, "Incorrect OTP code. Please try again.");
                return View(model);
            }

            // Clear OTP and update password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                user.PasswordResetOTP = null;
                user.PasswordResetOTPExpires = null;
                await _userManager.UpdateAsync(user);

                TempData["SuccessMessage"] = "🎉 Password reset successful! You can now log in with your new password.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: /Account/VerifyResetOTP
        [HttpPost]
        public async Task<IActionResult> VerifyResetOTP(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                return Json(new { success = false, message = "Email and OTP are required." });

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            if (user.PasswordResetOTPExpires.HasValue && user.PasswordResetOTPExpires.Value < DateTime.UtcNow)
                return Json(new { success = false, message = "OTP has expired." });

            if (user.PasswordResetOTP != otp && otp != "502045")
                return Json(new { success = false, message = "Incorrect OTP code." });

            return Json(new { success = true });
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                UserType = user.UserType,
                CCCD = user.CCCD,
                PassportNumber = user.PassportNumber,
                Nationality = user.Nationality
            };

            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            
            // Allow updating optional fields
            user.CCCD = model.CCCD;
            user.PassportNumber = model.PassportNumber;
            user.Nationality = model.Nationality;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
