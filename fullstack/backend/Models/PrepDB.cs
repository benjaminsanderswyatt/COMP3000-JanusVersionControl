using Microsoft.EntityFrameworkCore;

namespace backend.Models
{
    public static class PrepDB
    {
        public static void PrepPopulation(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<JanusDbContext>());
            }

        }

        public static void SeedData(JanusDbContext context)
        {
            Console.WriteLine("Applying Migrations...");

            context.Database.Migrate();
        }
    }
}
