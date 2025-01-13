using Microsoft.EntityFrameworkCore;
using backend.Models;


// Cleans up the token blacklist, removes tokens which have expired exery x days
public class PATBlacklistCleanupService : IHostedService, IDisposable
{

    private readonly JanusDbContext _janusDbContext;
    private Timer _timer;

    public PATBlacklistCleanupService(JanusDbContext janusDbContext)
    {
        _janusDbContext = janusDbContext;
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
        var timeAtStart = DateTime.UtcNow;


        // Find expired tokens
        var expiredTokens = await _janusDbContext.AccessTokenBlacklists
            .Where(t => t.Expires <= timeAtStart)
            .ToListAsync();


        if (expiredTokens.Any())
        {
            // Remove all expired tokens
            _janusDbContext.AccessTokenBlacklists.RemoveRange(expiredTokens);
            await _janusDbContext.SaveChangesAsync();
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
