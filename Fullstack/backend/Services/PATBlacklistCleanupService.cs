using backend.Models;
using Microsoft.EntityFrameworkCore;


// Cleans up the token blacklist, removes tokens which have expired exery x days
public class PATBlacklistCleanupService : IHostedService, IDisposable
{

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Timer _timer;

    public PATBlacklistCleanupService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }



    public Task StartAsync(CancellationToken cancellationToken)
    {
        string? timespan = Environment.GetEnvironmentVariable("BlacklistCleanupTime");

        if (string.IsNullOrEmpty(timespan) || !int.TryParse(timespan, out var intervalDays))
        {
            intervalDays = 30; // Default to 30 days
        }

        Console.WriteLine("PATBlacklistCleanupService will run every " + intervalDays + " days.");

        _timer = new Timer(CleanupExpiredTokens, null, TimeSpan.Zero, TimeSpan.FromDays(intervalDays)); // Run every x days
        return Task.CompletedTask;
    }



    private async void CleanupExpiredTokens(object state)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<JanusDbContext>();
            var timeAtStart = DateTime.UtcNow;


            // Find expired tokens
            var expiredTokens = await dbContext.AccessTokenBlacklists
                .Where(t => t.Expires <= timeAtStart)
                .ToListAsync();


            if (expiredTokens.Any())
            {
                // Remove all expired tokens
                dbContext.AccessTokenBlacklists.RemoveRange(expiredTokens);
                await dbContext.SaveChangesAsync();
            }
        }
    }



    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("PATBlacklistCleanupService is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }



    public void Dispose()
    {
        _timer?.Dispose();
    }
}
