using HRMS.Constants;
using HRMS.Data;
using HRMS.Services.Interfaces;
using HRMS.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            ApplicationDbContext context,
            IAuthService authService,
            IAuditService auditService,
            ILogger<LoginController> logger)
        {
            _context = context;
            _authService = authService;
            _auditService = auditService;
            _logger = logger;
        }

        // GET LOGIN
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel());
        }

        // POST LOGIN
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var authResult = await _authService.ValidateCredentialsAsync(model, cancellationToken);

            if (!authResult.Success || authResult.User == null)
            {
                ModelState.AddModelError(string.Empty, authResult.ErrorMessage ?? "Invalid username or password.");
                return View(model);
            }

            var user = authResult.User;
            var employee = authResult.Employee;
            if (employee == null)
            {
                return Content("Employee object is NULL");
            }


            var principal = _authService.BuildPrincipal(user, employee);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    IssuedUtc = DateTimeOffset.UtcNow,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            var employeeName = employee == null
                ? user.UserName ?? "Unknown"
                : $"{employee.FirstName ?? string.Empty} {employee.LastName ?? string.Empty}".Trim();

            HttpContext.Session.SetString("UserName", user.UserName ?? string.Empty);
            HttpContext.Session.SetString("RoleName", user.RoleName ?? "Employee");
            HttpContext.Session.SetInt32("UserId", user.UserId);

            if (employee != null)
            {
                HttpContext.Session.SetInt32(
                    "EmployeeId",
                    employee.EmployeeId);
            }

            HttpContext.Session.SetString("EmployeeName", employeeName);
            HttpContext.Session.SetString("EmployeeCode", employee?.EmployeeCode ?? string.Empty);

            await _auditService.LogAsync(
                "Login",
                "Authentication",
                "User logged in.",
                //user.UserName,
                employeeName,
                cancellationToken);

            _logger.LogInformation("User {UserName} logged in successfully.", user.UserName);

            return RedirectToAction("Index", "Dashboard");
        }

        // GET REGISTER
        [AllowAnonymous]
        public async Task<IActionResult> Register(CancellationToken cancellationToken)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var viewModel = new RegisterViewModel();
            await PopulateEmployeesAsync(viewModel, cancellationToken);

            return View(viewModel);
        }

        // POST REGISTER
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            await PopulateEmployeesAsync(model, cancellationToken);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model, cancellationToken);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to register user.");
                return View(model);
            }

            await _auditService.LogAsync(
                "Register",
                "Authentication",
                $"New user registered: {result.User?.UserName}",
                result.User?.UserName,
                cancellationToken);

            TempData["Success"] = "Registration completed. You can now sign in.";

            return RedirectToAction(nameof(Login));
        }

        // GET FORGOT PASSWORD
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new ForgotPasswordViewModel());
        }

        // POST FORGOT PASSWORD
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _auditService.LogAsync(
                "PasswordResetRequested",
                "Authentication",
                $"Password reset requested for username: {model.UserName}",
                "System",
                cancellationToken);

            TempData["Info"] = "If the account exists, contact HR/Admin for password reset support.";

            return RedirectToAction(nameof(Login));
        }

        // LOGOUT
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            //var userName = User.Identity?.Name;
            var employeeName = User.FindFirst(CustomClaimTypes.EmployeeName)?.Value
                   ?? User.Identity?.Name;

            await _auditService.LogAsync(
                "Logout",
                "Authentication",
                "User logged out.",
                //userName,
                employeeName,
                cancellationToken);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Login));
        }

        private async Task PopulateEmployeesAsync(RegisterViewModel model, CancellationToken cancellationToken)
        {
            var employees = await _context.Employees
                .Where(employee => !_context.Users.Any(user => user.EmployeeId == employee.EmployeeId))
                .OrderBy(employee => employee.FirstName)
                .Select(employee => new SelectListItem
                {
                    Value = employee.EmployeeId.ToString(),
                    Text = ($"{employee.FirstName ?? string.Empty} {employee.LastName ?? string.Empty}").Trim()
                })
                .ToListAsync(cancellationToken);

            model.Employees = employees;
        }
    }
}
