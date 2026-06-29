using HRMS.Constants;
using HRMS.Data;
using HRMS.Models;
using HRMS.Services.Interfaces;
using HRMS.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRMS.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<(bool Success, User? User, Employee? Employee, string? ErrorMessage)> ValidateCredentialsAsync(
            LoginViewModel model,
            CancellationToken cancellationToken = default)
        {
            var normalizedUserName = model.UserName.Trim();

            var user = await _context.Users
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(
                    x => x.UserName != null && x.UserName.ToLower() == normalizedUserName.ToLower(),
                    cancellationToken);

            if (user == null || !user.IsActive)
            {
                return (false, null, null, "Invalid username or password.");
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                _logger.LogWarning("User {UserName} has empty password hash.", user.UserName);
                return (false, null, null, "Invalid username or password.");
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            var legacyPlainTextMatch = verification == PasswordVerificationResult.Failed
                && user.PasswordHash == model.Password;

            if (verification == PasswordVerificationResult.Failed && !legacyPlainTextMatch)
            {
                return (false, null, null, "Invalid username or password.");
            }

            if (legacyPlainTextMatch || verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return (true, user, user.Employee, null);
        }

        public async Task<(bool Success, User? User, string? ErrorMessage)> RegisterAsync(
            RegisterViewModel model,
            CancellationToken cancellationToken = default)
        {
            var normalizedUserName = model.UserName.Trim();

            var userNameExists = await _context.Users.AnyAsync(
                x => x.UserName != null && x.UserName.ToLower() == normalizedUserName.ToLower(),
                cancellationToken);

            if (userNameExists)
            {
                return (false, null, "Username already exists.");
            }

            var employeeExists = await _context.Employees.AnyAsync(
                x => x.EmployeeId == model.EmployeeId,
                cancellationToken);

            if (!employeeExists)
            {
                return (false, null, "Selected employee does not exist.");
            }

            var employeeAlreadyMapped = await _context.Users.AnyAsync(
                x => x.EmployeeId == model.EmployeeId,
                cancellationToken);

            if (employeeAlreadyMapped)
            {
                return (false, null, "A login already exists for this employee.");
            }

            var user = new User
            { 
                EmployeeId = model.EmployeeId,
                UserName = normalizedUserName,
                RoleName = model.RoleName,
                IsActive = true
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return (true, user, null);
        }

        public ClaimsPrincipal BuildPrincipal(User user, Employee? employee)
        {
            var employeeName = employee == null
                ? user.UserName ?? "Unknown"
                : $"{employee.FirstName ?? string.Empty} {employee.LastName ?? string.Empty}".Trim();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Role, user.RoleName ?? "Employee"),
                new(CustomClaimTypes.EmployeeId, user.EmployeeId.ToString()),
                new(CustomClaimTypes.EmployeeName, employeeName),
                new(CustomClaimTypes.EmployeeCode, employee?.EmployeeCode ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}
