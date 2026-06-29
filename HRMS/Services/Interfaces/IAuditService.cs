namespace HRMS.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(
            string actionType,
            string moduleName,
            string description,
            string? userName = null,
            CancellationToken cancellationToken = default);
    }
}
