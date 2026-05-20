namespace PRN232.LMS.Services.Interfaces;

public interface IDatabaseBootstrapService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
