using backend.Models;
using Microsoft.EntityFrameworkCore;


public class DatabaseInitialiser : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitialiser(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<JanusDbContext>();

            try
            {
                Console.WriteLine("Applying migrations...");

                await dbContext.Database.MigrateAsync(cancellationToken);

                Console.WriteLine("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while applying database migrations: {ex.Message}");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}
