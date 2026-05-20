using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implements;

public class DatabaseBootstrapService(
    LmsDbContext dbContext,
    ILogger<DatabaseBootstrapService> logger) : IDatabaseBootstrapService
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 5;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
                await LmsDbSeeder.SeedAsync(dbContext, logger, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex, "Database initialization failed on attempt {Attempt}. Retrying...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        await dbContext.Database.MigrateAsync(cancellationToken);
        await LmsDbSeeder.SeedAsync(dbContext, logger, cancellationToken);
    }
}
