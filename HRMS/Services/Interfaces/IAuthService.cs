using HRMS.Models;
using HRMS.ViewModels.Auth;
using System.Security.Claims;

namespace HRMS.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, User? User, Employee? Employee, string? ErrorMessage)> ValidateCredentialsAsync(
            LoginViewModel model,
            CancellationToken cancellationToken = default);

        Task<(bool Success, User? User, string? ErrorMessage)> RegisterAsync(
            RegisterViewModel model,
            CancellationToken cancellationToken = default);

        ClaimsPrincipal BuildPrincipal(User user, Employee? employee);
    }
}
