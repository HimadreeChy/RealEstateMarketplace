using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RealEstateMarketplace.Data;
using RealEstateMarketplace.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RealEstateMarketplace.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // REGISTER
        // =========================

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            User user,
            string password)
        {
            // Every new registration is Buyer
            user.Role = "Buyer";

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "password",
                    "Password is required.");
            }

            // Server-generated values
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            bool emailExists = _context.Users
                .Any(u => u.Email == user.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email is already registered.");

                return View(user);
            }

            user.PasswordHash = HashPassword(password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Registration successful! Please login.";

            return RedirectToAction(nameof(Login));
        }


        // =========================
        // LOGIN
        // =========================

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Email and password are required.");

                return View();
            }

            string passwordHash = HashPassword(password);

            var user = await Task.FromResult(
                _context.Users.FirstOrDefault(u =>
                    u.Email == email &&
                    u.PasswordHash == passwordHash)
            );

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.FullName),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================
        // FORGOT PASSWORD
        // =========================

        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(
                    "email",
                    "Email is required.");

                return View();
            }

            var user = await Task.FromResult(
                _context.Users.FirstOrDefault(
                    u => u.Email == email)
            );

            if (user == null)
            {
                ModelState.AddModelError(
                    "email",
                    "No account found with this email.");

                return View();
            }

            // Generate secure reset token
            user.PasswordResetToken =
                Convert.ToBase64String(
                    RandomNumberGenerator.GetBytes(32));

            // Token valid for 30 minutes
            user.PasswordResetTokenExpiry =
                DateTime.Now.AddMinutes(30);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(ResetPassword),
                new
                {
                    token = user.PasswordResetToken
                });
        }


        // =========================
        // RESET PASSWORD
        // =========================

        // GET: Account/ResetPassword
        [HttpGet]
        public async Task<IActionResult> ResetPassword(
            string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return NotFound();
            }

            var user = await Task.FromResult(
                _context.Users.FirstOrDefault(
                    u => u.PasswordResetToken == token)
            );

            if (user == null)
            {
                TempData["ErrorMessage"] =
                    "Invalid password reset link.";

                return RedirectToAction(nameof(Login));
            }

            if (user.PasswordResetTokenExpiry == null ||
                user.PasswordResetTokenExpiry < DateTime.Now)
            {
                TempData["ErrorMessage"] =
                    "Password reset link has expired.";

                return RedirectToAction(nameof(Login));
            }

            ViewBag.Token = token;

            return View();
        }


        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            string token,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "password",
                    "Password is required.");
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(
                    "confirmPassword",
                    "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Token = token;
                return View();
            }

            var user = await Task.FromResult(
                _context.Users.FirstOrDefault(
                    u => u.PasswordResetToken == token)
            );

            if (user == null)
            {
                TempData["ErrorMessage"] =
                    "Invalid password reset request.";

                return RedirectToAction(nameof(Login));
            }

            if (user.PasswordResetTokenExpiry == null ||
                user.PasswordResetTokenExpiry < DateTime.Now)
            {
                TempData["ErrorMessage"] =
                    "Password reset link has expired.";

                return RedirectToAction(nameof(Login));
            }

            // Update password
            user.PasswordHash = HashPassword(password);

            // Invalidate reset token
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Password reset successful! Please login.";

            return RedirectToAction(nameof(Login));
        }


        // =========================
        // LOGOUT
        // =========================

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================
        // PASSWORD HASH
        // =========================

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();

            byte[] bytes =
                Encoding.UTF8.GetBytes(password);

            byte[] hash =
                sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}